using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Threading;


namespace MinVR {

    public class VRNetClient {
	
	    // unique identifiers for different network messages
	    static readonly byte[] INPUT_EVENTS_MSG = {1};
	    static readonly byte[] SWAP_BUFFERS_REQUEST_MSG = {2};
	    static readonly byte[] SWAP_BUFFERS_NOW_MSG = {3};
	
	    TcpClient client;
	    NetworkStream stream;

	    public VRNetClient(string serverIP, int serverPort) {
            // continue trying to connect until we have success
            bool success = false;
            while (!success) {
                try {
                    client = new TcpClient(AddressFamily.InterNetwork);
                    client.Connect(IPAddress.Parse(serverIP), serverPort);
                    stream = client.GetStream();
                    success = client.Connected;
                }
                catch (ArgumentNullException e) {
                    Debug.Log(String.Format("ArgumentNullException: {0}", e));
                    Console.WriteLine("ArgumentNullException: {0}", e);
                }
                catch (SocketException e) {
                    Debug.Log(String.Format("SocketException: {0}", e));
                    Console.WriteLine("SocketException: {0}", e);
                }
                catch (Exception e) {
                    Debug.Log(String.Format("Exception: {0}", e));
                    Console.WriteLine("Exception: {0}", e);
                }
                if (!success) {
                    Debug.Log("Having trouble connecting to the VRNetServer.  Trying again...");
                    Console.WriteLine("Having trouble connecting to the VRNetServer.  Trying again...");
                    Thread.Sleep(500);
                }
            }
	    }
	
	    ~VRNetClient() {
		    try {
			    // Close everything.
			    stream.Close();         
			    client.Close();         
		    } 
		    catch (ArgumentNullException e) {
			    Console.WriteLine("ArgumentNullException: {0}", e);
		    } 
		    catch (SocketException e) {
			    Console.WriteLine("SocketException: {0}", e);
		    }
	    }

        public void BrokenConnectionError() {
            Debug.Log("Network connection broken, shutting down.");
            Console.WriteLine("Network connection broken, shutting down.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(1);
#endif
        }

        public void SynchronizeInputEventsAcrossAllNodes(ref List<VREvent> inputEvents) {
		    // 1. send inputEvents to server
		    SendInputEvents(ref inputEvents);
		
		    // 2. receive and parse serverInputEvents
		    List<VREvent> serverInputEvents = new List<VREvent>();
		    WaitForAndReceiveInputEvents(ref serverInputEvents);
		
		    // 3. inputEvents = serverInputEvents
		    inputEvents = serverInputEvents;
	    }
	
	    public void SynchronizeSwapBuffersAcrossAllNodes() {
		    // 1. send a swap_buffers_request message to the server
		    SendSwapBuffersRequest();
		
		    // 2. wait for and receive a swap_buffers_now message from the server
		    WaitForAndReceiveSwapBuffersNow();
	    }
	
	    void SendSwapBuffersRequest() {
            if (!client.Connected) {
                BrokenConnectionError();
                return;
            }
            // this message consists only of a 1-byte header
            try {
                stream.Write(SWAP_BUFFERS_REQUEST_MSG, 0, 1);
            }
            catch (Exception e) {
                Console.WriteLine("Exception: {0}", e);
                BrokenConnectionError();
            }
        }

	    void SendInputEvents(ref List<VREvent> inputEvents) {
            // Debug.Log("SendInputEvents");

            // 1. send 1-byte message header
            if (!client.Connected) {
                BrokenConnectionError();
                return;
            }
            try {
                stream.Write(INPUT_EVENTS_MSG, 0, 1);
            }
            catch (Exception e) {
                Console.WriteLine("Exception: {0}", e);
                BrokenConnectionError();
            }

            // 2. create an XML-formatted string to hold all the inputEvents
            string xmlEvents = "<VRDataQueue num=\"" + inputEvents.Count + "\">";
		    foreach (VREvent inputEvent in inputEvents) {
			    xmlEvents += "<VRDataQueueItem timeStamp=\"0.0\">" + inputEvent.ToXML() + "</VRDataQueueItem>";
		    }
		    xmlEvents += "</VRDataQueue>";

		    // 3. send the size of the message data so receive will know how many bytes to expect
		    WriteInt32(xmlEvents.Length);

		    // 4. send the chars that make up xmlEvents string
		    byte[] bytes = Encoding.ASCII.GetBytes(xmlEvents);
            if (!client.Connected) {
                BrokenConnectionError();
                return;
            }
            try {
                stream.Write(bytes, 0, bytes.Length);
            }
            catch (Exception e) {
                Console.WriteLine("Exception: {0}", e);
                BrokenConnectionError();
            }
        }


	
	    void WaitForAndReceiveMessageHeader(byte [] messageID) {
		    byte[] receivedID = new byte[1];
		    while (receivedID[0] != messageID[0]) {
                int status = -1;
                if (!client.Connected) {
                    BrokenConnectionError();
                    return;
                }
                try {
                    status = stream.Read(receivedID, 0, 1);
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
			    else if ((status == 1) && (receivedID[0] != messageID[0])) {
				    Console.WriteLine("WaitForAndReceiveMessageHeader error: expected {0} got {1}", messageID[0], receivedID[0]);
                    return;
			    }
		    }
	    }
	
	    void WaitForAndReceiveSwapBuffersRequest() {
            // Debug.Log("WaitForAndReceiveSwapBuffersRequest");
            // this message consists only of a 1-byte header
            WaitForAndReceiveMessageHeader(SWAP_BUFFERS_REQUEST_MSG);
	    }
	
	    void WaitForAndReceiveSwapBuffersNow() {
            // Debug.Log("WaitForAndReceiveSwapBuffersNow");
            // this message consists only of a 1-byte header
            WaitForAndReceiveMessageHeader(SWAP_BUFFERS_NOW_MSG);
	    }
		
	    void WaitForAndReceiveInputEvents(ref List<VREvent> inputEvents) {
            // Debug.Log("WaitForAndReceiveInputEvents");

            // 1. receive 1-byte message header
            WaitForAndReceiveMessageHeader(INPUT_EVENTS_MSG);
		
		    // 2. receive int that tells us the size of the data portion of the message in bytes
		    Int32 dataSize = ReadInt32();
				
		    // 3. receive dataSize bytes, then decode these as InputEvents
		    byte[] buf2 = new byte[dataSize+1];
		    int status = ReceiveAll(ref buf2, dataSize);
		    if (status == -1) {
			    Console.WriteLine("WaitForAndReceiveInputEvents error reading data");
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
        }

	    int ReceiveAll(ref byte[] buf, int len) {
		    int total = 0;        // how many bytes we've received
		    int bytesleft = len; // how many we have left to receive
		    int n;    
		    while (total < len) {
                if (!client.Connected) {
                    BrokenConnectionError();
                    return -1;
                }
                try {
                    n = stream.Read(buf, total, bytesleft);
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

	
	    void WriteInt32(Int32 i) {
		    if (!BitConverter.IsLittleEndian) {
			    i = SwapEndianness(i);
		    }
		    byte[] buf = BitConverter.GetBytes(i);
            if (!client.Connected) {
                BrokenConnectionError();
                return;
            }
            try {
                stream.Write(buf, 0, 4);
            }
            catch (Exception e) {
                Console.WriteLine("Exception: {0}", e);
                BrokenConnectionError();
            }
        }

	    Int32 ReadInt32() {
		    byte[] buf = new byte[4];
		    int status = ReceiveAll(ref buf, 4);
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

	    static Int32 SwapEndianness(Int32 value) {
		    var b1 = (value >> 0) & 0xff;
		    var b2 = (value >> 8) & 0xff;
		    var b3 = (value >> 16) & 0xff;
		    var b4 = (value >> 24) & 0xff;		
		    return b1 << 24 | b2 << 16 | b3 << 8 | b4 << 0;
	    }	
    }

} // namespace MinVR
