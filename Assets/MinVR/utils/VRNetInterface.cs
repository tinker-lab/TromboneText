using System.Collections;
using System.Collections.Generic;

namespace MinVR {


    /** Servers and clients both implement this interface, but perform different tasks for each function. */
    public interface VRNetInterface {

        void SynchronizeInputEventsAcrossAllNodes(ref List<VREvent> inputEvents);

        void SynchronizeSwapBuffersAcrossAllNodes();

    }

}
