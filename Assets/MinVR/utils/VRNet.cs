using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace MinVR {

    public static class VRNet {

        // PART 1:  SENDING/RECEIVING SMALL CONTROL MESSAGES AND 1-BYTE MESSAGE HEADERS

        // unique identifiers for different network messages
        public static readonly byte[] INPUT_EVENTS_MSG = { 1 };
        public static readonly byte[] SWAP_BUFFERS_REQUEST_MSG = { 2 };
        public static readonly byte[] SWAP_BUFFERS_NOW_MSG = { 3 };


        public static void SendOneByteMessage(ref TcpClient client, byte[] message) {
            if (!client.Connected) {
                BrokenConnectionError();
                return;
            }
            // this message consists only of a 1-byte header
            try {
                client.GetStream().Write(message, 0, 1);
            }
            catch (Exception e) {
                Console.WriteLine("Exception: {0}", e);
                BrokenConnectionError();
            }
        }


        public static void SendSwapBuffersRequest(ref TcpClient client) {
            SendOneByteMessage(ref client, VRNet.SWAP_BUFFERS_REQUEST_MSG);
        }

        public static void SendSwapBuffersNow(ref TcpClient client) {
            SendOneByteMessage(ref client, VRNet.SWAP_BUFFERS_NOW_MSG);
        }


        // Blocks until the specific message specified is received
        public static void ReceiveOneByteMessage(ref TcpClient client, byte[] message) {
            byte[] received = new byte[1];
            while (received[0] != message[0]) {
                int status = -1;
                if (!client.Connected) {
                    BrokenConnectionError();
                    return;
                }
                try {
                    status = client.GetStream().Read(received, 0, 1);
                }
                catch (Exception e) {
                    Console.WriteLine("Exception: {0}", e);
                    BrokenConnectionError();
                    return;
                }
                if (status == -1) {
                    Console.WriteLine("WaitForAndReceiveMessageHeader failed");
                    return;
                }
                else if ((status == 1) && (received[0] != message[0])) {
                    Console.WriteLine("WaitForAndReceiveMessageHeader error: expected {0} got {1}", message[0], received[0]);
                    return;
                }
            }
        }

        public static void ReceiveSwapBuffersRequest(ref TcpClient client) {
            ReceiveOneByteMessage(ref client, VRNet.SWAP_BUFFERS_REQUEST_MSG);
        }

        public static void ReceiveSwapBuffersNow(ref TcpClient client) {
            ReceiveOneByteMessage(ref client, VRNet.SWAP_BUFFERS_NOW_MSG);
        }





        // PART 2:  LARGER MESSAGES FOR SYNCING INPUT EVENTS

        public static void SendEventData(ref TcpClient client, in List<VREvent> inputEvents) {
            // Debug.Log("SendInputEvents");

            // 1. send 1-byte message header
            if (!client.Connected) {
                BrokenConnectionError();
                return;
            }
            try {
                client.GetStream().Write(VRNet.INPUT_EVENTS_MSG, 0, 1);
            }
            catch (Exception e) {
                Console.WriteLine("Exception: {0}", e);
                BrokenConnectionError();
            }


            // 2. send event data
            if (!client.Connected) {
                BrokenConnectionError();
                return;
            }
            try {
                using (MemoryStream ms = new MemoryStream()) {
                    using (BinaryWriter bw = new BinaryWriter(ms)) {
                        bw.Write(inputEvents.Count);
                        foreach (VREvent inputEvent in inputEvents) {
                            inputEvent.ToBinary(bw);
                        }
                        byte[] bytes = ms.ToArray();
                        WriteInt32(ref client, bytes.Length);
                        client.GetStream().Write(bytes, 0, bytes.Length);
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine("Exception: {0}", e);
                BrokenConnectionError();
            }
            
            /**
            // 2. create an XML-formatted string to hold all the inputEvents
            string xmlEvents = "<VRDataQueue num=\"" + inputEvents.Count + "\">";
            foreach (VREvent inputEvent in inputEvents) {
                xmlEvents += "<VRDataQueueItem timeStamp=\"0.0\">" + inputEvent.ToXML() + "</VRDataQueueItem>";
            }
            xmlEvents += "</VRDataQueue>";

            // 3. send the size of the message data so receive will know how many bytes to expect
            WriteInt32(ref client, xmlEvents.Length);

            // 4. send the chars that make up xmlEvents string
            byte[] bytes = Encoding.ASCII.GetBytes(xmlEvents);
            if (!client.Connected) {
                BrokenConnectionError();
                return;
            }
            try {
                client.GetStream().Write(bytes, 0, bytes.Length);
            }
            catch (Exception e) {
                Console.WriteLine("Exception: {0}", e);
                BrokenConnectionError();
            }
            **/
        }


        public static void ReceiveEventData(ref TcpClient client, ref List<VREvent> inputEvents) {
            // Debug.Log("WaitForAndReceiveInputEvents");

            // 1. receive 1-byte message header
            ReceiveOneByteMessage(ref client, VRNet.INPUT_EVENTS_MSG);

            // 2. receive event data
            try {
                int dataSize = ReadInt32(ref client);
                byte[] bytes = new byte[dataSize];
                int status = ReceiveAll(ref client, ref bytes, dataSize);
                if (status == -1) {
                    Console.WriteLine("ReceiveEventData error reading data");
                    return;
                }

                using (MemoryStream ms = new MemoryStream(bytes)) {
                    using (BinaryReader br = new BinaryReader(ms)) {
                        int nEvents = br.ReadInt32();
                        for (int i = 0; i < nEvents; i++) {
                            inputEvents.Add(VREvent.FromBinary(br));
                        }
                    }
                }
            }
            catch (Exception e) {
                Debug.Log("Exception: " + e);
                Console.WriteLine("Exception: {0}", e);
                BrokenConnectionError();
            }
            

            /**
            // 2. receive int that tells us the size of the data portion of the message in bytes
            Int32 dataSize = ReadInt32(ref client);

            // 3. receive dataSize bytes, then decode these as InputEvents
            byte[] buf2 = new byte[dataSize + 1];
            int status = ReceiveAll(ref client, ref buf2, dataSize);
            if (status == -1) {
                Console.WriteLine("ReceiveEventData error reading data");
                return;
            }
            buf2[dataSize] = 0;

            // buf2 is the XML string that contains all the events.
            string serializedQueue = System.Text.Encoding.UTF8.GetString(buf2);
            //Debug.Log("Queue = " + serializedQueue);

            // Extract the VRDataQueue object
            Dictionary<string, string> queueProps = new Dictionary<string, string>();
            string queueContent = string.Empty;
            string queueLeftover = string.Empty;
            bool queueSuccess = XMLUtils.GetXMLField(serializedQueue, "VRDataQueue", ref queueProps, ref queueContent, ref queueLeftover);
            if (!queueSuccess) {
                Debug.Log("Error decoding VRDataQueue");
                return;
            }

            // The queue contents are VRDataItems, extract each one
            int nItems = Convert.ToInt32(queueProps["num"]);

            //Debug.Log("Num = " + nItems);
            //Debug.Log(queueContent);

            for (int i = 0; i < nItems; i++) {
                Dictionary<string, string> itemProps = new Dictionary<string, string>();
                string itemContent = string.Empty;
                string itemLeftover = string.Empty;
                bool itemSuccess = XMLUtils.GetXMLField(queueContent, "VRDataQueueItem", ref itemProps, ref itemContent, ref itemLeftover);
                if (!itemSuccess) {
                    Debug.Log("Error decoding VRDataQueueItem #" + i);
                    return;
                }

                // Create a new VREvent from the content of this item
                //Debug.Log("Item Content = " + itemContent);
                VREvent e = new VREvent(ref itemContent);
                inputEvents.Add(e);

                // Update the content to point to the next item if there is one
                queueContent = itemLeftover;
            }
            **/
        }




        // PART 3:  LOWER-LEVEL NETWORK ROUTINES

        // Blocks and continues reading until len bytes are read into buf
        public static int ReceiveAll(ref TcpClient client, ref byte[] buf, int len) {
            int total = 0;        // how many bytes we've received
            int bytesleft = len; // how many we have left to receive
            int n;
            while (total < len) {
                if (!client.Connected) {
                    BrokenConnectionError();
                    return -1;
                }
                try {
                    n = client.GetStream().Read(buf, total, bytesleft);
                    total += n;
                    bytesleft -= n;
                }
                catch (Exception e) {
                    Console.WriteLine("Exception: {0}", e);
                    BrokenConnectionError();
                    return -1;
                }
            }
            return total; // return -1 on failure, total on success
        }


        public static void WriteInt32(ref TcpClient client, Int32 i) {
            if (!BitConverter.IsLittleEndian) {
                i = SwapEndianness(i);
            }
            byte[] buf = BitConverter.GetBytes(i);
            if (!client.Connected) {
                BrokenConnectionError();
                return;
            }
            try {
                client.GetStream().Write(buf, 0, 4);
            }
            catch (Exception e) {
                Console.WriteLine("Exception: {0}", e);
                BrokenConnectionError();
            }
        }

        public static Int32 ReadInt32(ref TcpClient client) {
            byte[] buf = new byte[4];
            int status = ReceiveAll(ref client, ref buf, 4);
            if (status == -1) {
                Console.WriteLine("ReadInt32() error reading data");
                return 0;
            }
            Int32 i = BitConverter.ToInt32(buf, 0);
            if (!BitConverter.IsLittleEndian) {
                i = SwapEndianness(i);
            }
            return i;
        }

        public static Int32 SwapEndianness(Int32 value) {
            var b1 = (value >> 0) & 0xff;
            var b2 = (value >> 8) & 0xff;
            var b3 = (value >> 16) & 0xff;
            var b4 = (value >> 24) & 0xff;
            return b1 << 24 | b2 << 16 | b3 << 8 | b4 << 0;
        }


        public static void BrokenConnectionError() {
            Debug.Log("Network connection broken, shutting down.");
            Console.WriteLine("Network connection broken, shutting down.");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit(1);
            #endif
        }


    } // end class

} // end namespace
