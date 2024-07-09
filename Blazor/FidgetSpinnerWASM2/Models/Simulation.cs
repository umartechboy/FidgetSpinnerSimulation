using MudBlazor;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using System.ComponentModel;
using static MudBlazor.Defaults.Classes;
using System;
using System.Text;
using Typography.OpenFont.Tables;
namespace FidgetSpinnerWASM2.Models
{
    public class Simulation
    {
        public List<Spinner> spinners = new List<Spinner>();
        public event EventHandler OnRequestToDraw;
        public event EventHandler OnRequestToUpdateState;
        public double t = 0;
        public double dt = 0.0001;
        public List<double> times = new List<double>();
        public List<SpinnerSimResult> results = new ();
        public float mu { get; set; } = (float)(4 * Math.PI * Math.Pow(10, -7));
        public bool magnetsAreHorizontal { get; set; } = false;
        public bool canStep { get; set; } = false;
        public void Reset()
        {

        }
        public void Pause()
        {
            canStep = false;
        }
        public void Play()
        {
            canStep = true;
        }
        public void Step()
        {
            double maxdth = 0;
            foreach (var spinner in spinners)
            {
                double alpha = 0;
                double tauF = 0;
                if (spinner.IsPowered)
                {
                    // w is fixed
                    tauF = 0;
                    alpha = 0;
                    spinner.SimResult.torques.Add(0);
                    spinner.SimResult.netTorques.Add(0);
                }
                else
                {
                    Vector3 tauOnii = new Vector3();
                    foreach (var actor in spinners)
                    {
                        if (spinner == actor)
                            continue;
                        if (spinner.IsPowered)
                            continue;
                        tauOnii += calculateTorque(spinner, actor);
                    }
                    //calculate the torque due to friction
                    tauF = -spinner.w * spinner.B;
            // calculate angular velocities
            alpha = (tauOnii.Z + tauF) / spinner.TotalI(); // We only need the vertical component of rotation

            spinner.a = alpha;
                    spinner.tau = tauOnii.Z + tauF;
                    spinner.w = spinner.w + alpha * dt;
                    spinner.SimResult.torques.Add(tauOnii.Z);
                    spinner.SimResult.netTorques.Add(tauOnii.Z + tauF);
                }

                spinner.SimResult.frictions.Add(tauF);
                spinner.SimResult.accelerations.Add(alpha);
                spinner.SimResult.velocities.Add(spinner.w);
                spinner.SimResult.kineticEnergy.Add(1 / 2 * spinner.TotalI() * spinner.w * spinner.w);
            }
            foreach (var spinner in spinners)
            {
                // we have the velocities now, calculate final positions

                spinner.th = spinner.th + spinner.w * dt;
                spinner.SimResult.displacements.Add(spinner.th);
                if (spinner.w * dt > maxdth)
                    maxdth = spinner.w * dt;
            }

            // fine tune dt;
            dt = 0.1 / maxdth;
            if (dt > 0.002)
                dt = 0.002;
            times.Add(t);
            
            t += dt;
        }
        // Calculates the torque on spinner 1 by spinner 2
        Vector3 calculateTorque(Spinner spinner1, Spinner spinner2)
        {

            Vector3 totalTorque = new Vector3();
            double thD1 = 2 * Math.PI / spinner1.Magnets.Count; // angle between magnets on spinner 1
            double thD2 = 2 * Math.PI / spinner2.Magnets.Count; // angle between magnets on spinner 2
            for (int is1 = 0; is1 < spinner1.Magnets.Count; is1++) // itereate through spinner 1 magnets
            {
                for (int is2 = 0; is2 < spinner2.Magnets.Count; is2++) // itereate through spinner 2 magnets
                {
                    double thI1 = spinner1.th + thD1 * is1; // angle of magnet ith on spinner 1 in global f.o.r
                    double thI2 = spinner2.th + thD2 * is2; // angle of magnet ith on spinner 2 in global f.o.r

                    // for simplicity, lets extract the magnets
                    var magnet1 = spinner1.Magnets[is1];
                    var magnet2 = spinner2.Magnets[is2];


                    // to calculate torque, we need the center of spinner and positions
                    // of the two magents
                    var center1 = spinner1.Position;
                    var position1 = new Vector3((float)(spinner1.R * Math.Cos(thI1)), (float)(spinner1.R * Math.Sin(thI1)), 0) + spinner1.Position;
                    var position2 = new Vector3((float)(spinner2.R * Math.Cos(thI2)), (float)(spinner2.R * Math.Sin(thI2)), 0) + spinner2.Position;


                    // calculate moment arm vector
                    var r = position1 - center1;

                    // calculate force vector
                    var d = position2 - position1; // this vector has the right direction but not the right length for force.                                                                                        // find out the unit vector in this direction first
                    var d_mag = d.Length();
                    var F = (float)(3.0D / 4.0D * mu * magnet1.moment * magnet2.moment / Math.PI / Math.Pow(d_mag, 5)) * d;
                    var m1 = magnet1.moment * Vector3.UnitZ;
                    var m2 = magnet2.moment * Vector3.UnitZ;
                    if (magnetsAreHorizontal)
                    {
                        Vector3 makeVector(double angle) => new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0);

                        m1 = magnet1.moment * makeVector(thD1);
                        m2 = magnet1.moment * makeVector(thD2);
                    }
                    var F2 = 3 * mu / (4 * (float)Math.PI * (float)Math.Pow(d_mag, 5)) * (
                        + Vector3.Dot(m1, d) * m2 
                        + Vector3.Dot(m2, d) * m1 
                        + Vector3.Dot(m1, m2) * d 
                        - 5 * Vector3.Dot(m1, d) * Vector3.Dot(m1, d) * d / (float)Math.Pow(d_mag, 2));
                  
                    if (magnet1.Polarity == magnet2.Polarity)
                        F *= -1;
                    var tau = Vector3.Cross(r, F);
                    totalTorque = totalTorque + tau;
                }
            }
            return totalTorque;
        }
    }
    public class SpinnerSimResult
    {
        public List<double> torques = new List<double>();
        public List<double> netTorques = new List<double>();
        public List<double> frictions = new List<double>();
        public List<double> accelerations = new List<double>();
        public List<double> velocities = new List<double>();
        public List<double> displacements = new List<double>();
        public List<double> kineticEnergy = new List<double>();
    }
    public enum ForceCalculation
    {
        Characteristics,
        SingleConstant,
        ExperimentalData
    }
}
