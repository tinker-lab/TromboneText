using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;


namespace MinVR {

    /** VREvents have a Name and zero or more named data fields.  For example, an "event" could be
     * as simple as the name "MenuButton1Pressed" with no data attached, or it could be a multitouch event named
     * "Touch1_Move" with data fields like:  Position = (x, y); Delta = (x, y); Velocity = (x, y); Pressure = p.
     * Tracker_Move events will have data fields that describe the new position and orientation of the tracker.
     */
    public class VREvent {

        private string _name;

        private Dictionary<string, byte> _byteFields = new Dictionary<string, byte>();
        private Dictionary<string, byte[]> _byteArrayFields = new Dictionary<string, byte[]>();

        private Dictionary<string, int> _intFields = new Dictionary<string, int>();
        private Dictionary<string, int[]> _intArrayFields = new Dictionary<string, int[]>();

        private Dictionary<string, float> _floatFields = new Dictionary<string, float>();
        private Dictionary<string, float[]> _floatArrayFields = new Dictionary<string, float[]>();

        private Dictionary<string, string> _stringFields = new Dictionary<string, string>();
        private Dictionary<string, string[]> _stringArrayFields = new Dictionary<string, string[]>();


        // To construct VREvents from code, use this constructor and then call AddData to
        // add data fields
        public VREvent(string name) {
            _name = name;
        }


        // Creates a VREvent from an XML-formatted description.  xmlDescription is modified as part of the constructor.
        // Following the current C++ implementation of MinVR, events are serialized in an XML defined by the C++
        // VRDataIndex class.  This constructor pops the first <MyEventName>...</MyEventName> field off of xmlDescription
        // to create the event and xmlDescription is set to whatever remains in the string.
        public VREvent(ref string xmlDescription) {
            // TODO 1: It might be faster to use C#'s XMLReader object to parse the XML.  The XMLUtils class
            // was never intended to be used during the rendering loop, just for config files.

            // TODO 2: It might be even faster to convert the entire serialization scheme to use binary!

            _name = XMLUtils.GetNextXMLFieldName(xmlDescription);

            Dictionary<string, string> props = new Dictionary<string, string>();
            string xmlData = string.Empty;
            string xmlRemaining = string.Empty;
            bool success = XMLUtils.GetXMLField(xmlDescription, _name, ref props, ref xmlData, ref xmlRemaining);
            if (!success) {
                Debug.Log("Error decoding VRDataIndex");
                return;
            }

            string nextField = XMLUtils.GetNextXMLFieldName(xmlData);
            while (nextField != string.Empty) {
                string datumValue = string.Empty;
                string xmlDataRemaining = string.Empty;
                success = XMLUtils.GetXMLField(xmlData, nextField, ref props, ref datumValue, ref xmlDataRemaining);
                if (!success) {
                    Debug.Log("Error decoding VRDatum named " + nextField);
                    return;
                }

                char[] separatingChars = { ',' };
                if (props["type"] == "int") {
                    //Debug.Log ("Got int: " + nextField + "=" + datumValue);
                    AddData(nextField, Convert.ToInt32(datumValue));
                }
                else if (props["type"] == "float") {
                    //Debug.Log ("Got float: " + nextField + "=" + datumValue);
                    AddData(nextField, Convert.ToSingle(datumValue));
                }
                else if (props["type"] == "string") {
                    //Debug.Log ("Got string: " + nextField + "=" + datumValue);
                    AddData(nextField, datumValue);
                }
                else if (props["type"] == "intarray") {
                    //Debug.Log ("Got intarray: " + nextField + "=" + datumValue);
                    string[] elements = datumValue.Split(separatingChars, System.StringSplitOptions.RemoveEmptyEntries);
                    int[] intarray = new int[elements.Length];
                    for (int i = 0; i < elements.Length; i++) {
                        intarray[i] = Convert.ToInt32(elements[i]);
                    }
                    AddData(nextField, intarray);
                }
                else if (props["type"] == "floatarray") {
                    //Debug.Log ("Got floatarray: " + nextField + "=" + datumValue);
                    string[] elements = datumValue.Split(separatingChars, System.StringSplitOptions.RemoveEmptyEntries);
                    float[] floatarray = new float[elements.Length];
                    for (int i = 0; i < elements.Length; i++) {
                        floatarray[i] = Convert.ToSingle(elements[i]);
                    }
                    AddData(nextField, floatarray);
                }
                else if (props["type"] == "stringarray") {
                    //Debug.Log ("Got stringarray: " + nextField + "=" + datumValue);
                    string[] elements = datumValue.Split(separatingChars, System.StringSplitOptions.RemoveEmptyEntries);
                    string[] strarray = new string[elements.Length];
                    for (int i = 0; i < elements.Length; i++) {
                        strarray[i] = elements[i];
                    }
                    AddData(nextField, strarray);
                }
                else {
                    Debug.Log("Unknown VRDatum type: " + props["type"]);
                }

                xmlData = xmlDataRemaining;
                nextField = XMLUtils.GetNextXMLFieldName(xmlData);
            }

            xmlDescription = xmlRemaining;
        }

        public string Name {
            get {
                return _name;
            }
            set {
                _name = value;
            }
        }


        public void AddData(string name, byte data) {
            _byteFields.Add(name, data);
        }

        public void AddData(string name, byte[] data) {
            _byteArrayFields.Add(name, data);
        }

        public void AddData(string name, int data) {
            _intFields.Add(name, data);
        }

        public void AddData(string name, int[] data) {
            _intArrayFields.Add(name, data);
        }

        public void AddData(string name, float data) {
            _floatFields.Add(name, data);
        }

        public void AddData(string name, float[] data) {
            _floatArrayFields.Add(name, data);
        }

        public void AddData(string name, string data) {
            _stringFields.Add(name, data);
        }

        public void AddData(string name, string[] data) {
            _stringArrayFields.Add(name, data);
        }



        public bool ContainsByteField(string name) {
            return _byteFields.ContainsKey(name);
        }

        public bool ContainsByteArrayField(string name) {
            return _byteArrayFields.ContainsKey(name);
        }

        public bool ContainsIntField(string name) {
            return _intFields.ContainsKey(name);
        }

        public bool ContainsIntArrayField(string name) {
            return _intArrayFields.ContainsKey(name);
        }

        public bool ContainsFloatField(string name) {
            return _floatFields.ContainsKey(name);
        }

        public bool ContainsFloatArrayField(string name) {
            return _floatArrayFields.ContainsKey(name);
        }

        public bool ContainsStringField(string name) {
            return _stringFields.ContainsKey(name);
        }

        public bool ContainsStringArrayField(string name) {
            return _stringArrayFields.ContainsKey(name);
        }



        public byte GetByteData(string name) {
            return _byteFields[name];
        }

        public byte[] GetByteArrayData(string name) {
            return _byteArrayFields[name];
        }

        public int GetIntData(string name) {
            return _intFields[name];
        }

        public int[] GetIntArrayData(string name) {
            return _intArrayFields[name];
        }

        public float GetFloatData(string name) {
            return _floatFields[name];
        }

        public float[] GetFloatArrayData(string name) {
            return _floatArrayFields[name];
        }

        public string GetStringData(string name) {
            return _stringFields[name];
        }

        public string[] GetStringArrayData(string name) {
            return _stringArrayFields[name];
        }



        public override string ToString() {
            string s = "Name:";
            s += " " + _name + "     ";
            s += "Data: ";

            foreach (KeyValuePair<string, byte> entry in _byteFields) {
                s += entry.Key + "(byte)=" + entry.Value + "     ";
            }
            foreach (KeyValuePair<string, byte[]> entry in _byteArrayFields) {
                s += entry.Key + "(byte[])=";
                for (int i=0; i<entry.Value.Length; i++) {
                    if (i > 0) s += ",";
                    s += entry.Value[i];
                }
                s += "     ";
            }
            foreach (KeyValuePair<string, int> entry in _intFields) {
                s += entry.Key + "(int)=" + entry.Value + "     ";
            }
            foreach (KeyValuePair<string, int[]> entry in _intArrayFields) {
                s += entry.Key + "(int[])=";
                for (int i = 0; i < entry.Value.Length; i++) {
                    if (i > 0) s += ",";
                    s += entry.Value[i];
                }
                s += "     ";
            }
            foreach (KeyValuePair<string, float> entry in _floatFields) {
                s += entry.Key + "(float)=" + entry.Value + "     ";
            }
            foreach (KeyValuePair<string, float[]> entry in _floatArrayFields) {
                s += entry.Key + "(float[])=";
                for (int i = 0; i < entry.Value.Length; i++) {
                    if (i > 0) s += ",";
                    s += entry.Value[i];
                }
                s += "     ";
            }
            foreach (KeyValuePair<string, string> entry in _stringFields) {
                s += entry.Key + "(string)=" + entry.Value + "     ";
            }
            foreach (KeyValuePair<string, string[]> entry in _stringArrayFields) {
                s += entry.Key + "(string[])=";
                for (int i = 0; i < entry.Value.Length; i++) {
                    if (i > 0) s += ",";
                    s += entry.Value[i];
                }
                s += "     ";
            }

            return s;
        }


        public static VREvent FromBinary(BinaryReader br) {
            string eName = br.ReadString();
            VREvent e = new VREvent(eName);

            int nByteFields = br.ReadInt32();
            for (int i=0; i<nByteFields; i++) {
                string fName = br.ReadString();
                byte data = br.ReadByte();
                e.AddData(fName, data);
            }

            int nByteArrayFields = br.ReadInt32();
            for (int i=0; i<nByteArrayFields; i++) {
                string fName = br.ReadString();
                int nEntries = br.ReadInt32();
                byte[] data = new byte[nEntries];
                for (int j=0; j<nEntries; j++) {
                    data[j] = br.ReadByte();
                }
                e.AddData(fName, data);
            }


            int nIntFields = br.ReadInt32();
            for (int i = 0; i < nIntFields; i++) {
                string fName = br.ReadString();
                int data = br.ReadInt32();
                e.AddData(fName, data);
            }

            int nIntArrayFields = br.ReadInt32();
            for (int i = 0; i < nIntArrayFields; i++) {
                string fName = br.ReadString();
                int nEntries = br.ReadInt32();
                int[] data = new int[nEntries];
                for (int j = 0; j < nEntries; j++) {
                    data[j] = br.ReadInt32();
                }
                e.AddData(fName, data);
            }


            int nFloatFields = br.ReadInt32();
            for (int i = 0; i < nFloatFields; i++) {
                string fName = br.ReadString();
                float data = br.ReadSingle();
                e.AddData(fName, data);
            }

            int nFloatArrayFields = br.ReadInt32();
            for (int i = 0; i < nFloatArrayFields; i++) {
                string fName = br.ReadString();
                int nEntries = br.ReadInt32();
                float[] data = new float[nEntries];
                for (int j = 0; j < nEntries; j++) {
                    data[j] = br.ReadSingle();
                }
                e.AddData(fName, data);
            }


            int nStringFields = br.ReadInt32();
            for (int i = 0; i < nStringFields; i++) {
                string fName = br.ReadString();
                string data = br.ReadString();
                e.AddData(fName, data);
            }

            int nStringArrayFields = br.ReadInt32();
            for (int i = 0; i < nStringArrayFields; i++) {
                string fName = br.ReadString();
                int nEntries = br.ReadInt32();
                string[] data = new string[nEntries];
                for (int j = 0; j < nEntries; j++) {
                    data[j] = br.ReadString();
                }
                e.AddData(fName, data);
            }

            return e;
        }


        public void ToBinary(BinaryWriter bw) {
            bw.Write(_name);

            bw.Write(_byteFields.Count);
            foreach (KeyValuePair<string, byte> entry in _byteFields) {
                bw.Write(entry.Key);
                bw.Write(entry.Value);
            }

            bw.Write(_byteArrayFields.Count);
            foreach (KeyValuePair<string, byte[]> entry in _byteArrayFields) {
                bw.Write(entry.Key);
                bw.Write(entry.Value.Length);
                for (int i = 0; i < entry.Value.Length; i++) {
                    bw.Write(entry.Value[i]);
                }
            }

            bw.Write(_intFields.Count);
            foreach (KeyValuePair<string, int> entry in _intFields) {
                bw.Write(entry.Key);
                bw.Write(entry.Value);
            }

            bw.Write(_intArrayFields.Count);
            foreach (KeyValuePair<string, int[]> entry in _intArrayFields) {
                bw.Write(entry.Key);
                bw.Write(entry.Value.Length);
                for (int i = 0; i < entry.Value.Length; i++) {
                    bw.Write(entry.Value[i]);
                }
            }

            bw.Write(_floatFields.Count);
            foreach (KeyValuePair<string, float> entry in _floatFields) {
                bw.Write(entry.Key);
                bw.Write(entry.Value);
            }

            bw.Write(_floatArrayFields.Count);
            foreach (KeyValuePair<string, float[]> entry in _floatArrayFields) {
                bw.Write(entry.Key);
                bw.Write(entry.Value.Length);
                for (int i = 0; i < entry.Value.Length; i++) {
                    bw.Write(entry.Value[i]);
                }
            }

            bw.Write(_stringFields.Count);
            foreach (KeyValuePair<string, string> entry in _stringFields) {
                bw.Write(entry.Key);
                bw.Write(entry.Value);
            }

            bw.Write(_stringArrayFields.Count);
            foreach (KeyValuePair<string, string[]> entry in _stringArrayFields) {
                bw.Write(entry.Key);
                bw.Write(entry.Value.Length);
                for (int i = 0; i < entry.Value.Length; i++) {
                    bw.Write(entry.Value[i]);
                }
            }
        }


        public string ToXML() {
            string s = "<" + _name + " type=\"container\">";


            foreach (KeyValuePair<string, byte> entry in _byteFields) {
                s += "<" + entry.Key + " type=\"char\">" + entry.Value + "</" + entry.Key + ">";
            }
            foreach (KeyValuePair<string, byte[]> entry in _byteArrayFields) {
                s += "<" + entry.Key + " type=\"chararray\">";
                for (int i = 0; i < entry.Value.Length; i++) {
                    if (i > 0) s += ",";
                    s += entry.Value[i];
                }
                s += "</" + entry.Key + ">";
            }
            foreach (KeyValuePair<string, int> entry in _intFields) {
                s += "<" + entry.Key + " type=\"int\">" + entry.Value + "</" + entry.Key + ">";
            }
            foreach (KeyValuePair<string, int[]> entry in _intArrayFields) {
                s += "<" + entry.Key + " type=\"intarray\">";
                for (int i = 0; i < entry.Value.Length; i++) {
                    if (i > 0) s += ",";
                    s += entry.Value[i];
                }
                s += "</" + entry.Key + ">";
            }
            foreach (KeyValuePair<string, float> entry in _floatFields) {
                s += "<" + entry.Key + " type=\"float\">" + entry.Value + "</" + entry.Key + ">";
            }
            foreach (KeyValuePair<string, float[]> entry in _floatArrayFields) {
                s += "<" + entry.Key + " type=\"floatarray\">";
                for (int i = 0; i < entry.Value.Length; i++) {
                    if (i > 0) s += ",";
                    s += entry.Value[i];
                }
                s += "</" + entry.Key + ">";
            }
            foreach (KeyValuePair<string, string> entry in _stringFields) {
                s += "<" + entry.Key + " type=\"string\">" + entry.Value + "</" + entry.Key + ">";
            }
            foreach (KeyValuePair<string, string[]> entry in _stringArrayFields) {
                s += "<" + entry.Key + " type=\"stringarray\">";
                for (int i = 0; i < entry.Value.Length; i++) {
                    if (i > 0) s += ",";
                    s += entry.Value[i];
                }
                s += "</" + entry.Key + ">";
            }


            s += "</" + _name + ">";
            return s;
        }


    }

} // namespace MinVR
