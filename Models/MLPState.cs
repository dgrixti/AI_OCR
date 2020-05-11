using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AI_OCR.Models
{

    /*
     *  Used for saving the MLP entire state to file.
     */
    public class MLPState
    {
        public float[] inputNeuronsArr { get; set; }
        public float[] hiddenNeuronsArr { get; set; }
        public float[] outputNeuronsArr { get; set; }
        public float[] weightsArr { get; set; }
        public float[] tempWeightsArr { get; set; }
        public float[] prevWeightsArr { get; set; }

        public int inputs { get; set; }
        public int outputs { get; set; }
        public int hiddenLayers { get; set; }
        public int hiddenNeurons { get; set; }

        public float meanSqErr { get; set; }
        public int epochs { get; set; }
        public float error { get; set; }
    }
}
