using MudBlazor;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using System.ComponentModel;
using static MudBlazor.Defaults.Classes;
using System;
using System.Text;

namespace FidgetSpinnerWASM2.Models
{
    public class Simulation
    {
        public List<Spinner> spinners = new List<Spinner>();
        public event EventHandler OnRequestToDraw;
        public event EventHandler OnRequestToUpdateState;
        public double t = 0;
        public double dt = 0.0001;
        List<double> times = new List<double>();
        List<double> torques = new List<double>();
        List<double> netTorques = new List<double>();
        List<double> frictions = new List<double>();
        List<double> accelerations = new List<double>();
        List<double> velocities = new List<double>();
        List<double> displacements = new List<double>();
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
            foreach (var spinner1 in spinners)
            {
                Vector3 tauOnii = new Vector3();
                foreach (var spinner2 in spinners)
                {
                    if (spinner1 == spinner2)
                        continue;
                    if (spinner1.IsPowered)
                        continue;
                    tauOnii = tauOnii + calculateTorque(spinner1, spinner2);
                }
                double alpha = 0;
                double tauF = 0;
                if (spinner1.IsPowered)
                {
                    // w is fixed
                    tauF = 0;
                    alpha = 0;
                    torques.Add(0);
                    netTorques.Add(0);
                }
                else
                {
            //calculate the torque due to friction
            tauF = -spinner1.w * spinner1.B;
            // calculate angular velocities
            alpha = (tauOnii.Z + tauF) / spinner1.I; // We only need the vertical component of rotation

            spinner1.a = alpha;
                    spinner1.tau = tauOnii.Z + tauF;
                    spinner1.w = spinner1.w + alpha * dt;
                    torques.Add(tauOnii.Z);
                    netTorques.Add(tauOnii.Z + tauF);
                }

                frictions.Add(tauF);
                accelerations.Add(alpha);
                velocities.Add(spinner1.w);
            }
            foreach (var spinner in spinners)
            {
                // we have the velocities now, calculate final positions

                spinner.th = spinner.th + spinner.w * dt;
                displacements.Add(spinner.th);
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
        // Calculates the torque on spinner 1 by spinner 1
        Vector3 calculateTorque(Spinner spinner1, Spinner spinner2)
        {

            Vector3 totalTorque = new Vector3();
            double thD1 = 2 * Math.PI / spinner1.Magnets.Count; // angle between magnets on spinner 1
            double thD2 = 2 * Math.PI / spinner2.Magnets.Count; // angle between magnets on spinner 2
            for (int is1 = 0; is1 < spinner1.Magnets.Count; is1++) // itereate through spinner 1 magnets
            {
                for (int is2 = 0; is2 < spinner2.Magnets.Count; is2++) // itereate through spinner 2 magnets
                {
                    double thI1 = spinner1.th + thD1 * (is1 - 1); // angle of magnet ith on spinner 1 in global f.o.r
                    double thI2 = spinner2.th + thD2 * (is2 - 1); // angle of magnet ith on spinner 2 in global f.o.r

                    // for simplicity, lets extract the magnets
                    var magnet1 = spinner1.Magnets[is1];
                    var magnet2 = spinner1.Magnets[is2];


                    // to calculate torque, we need the center of spinner and positions
                    // of the two magents
                    var center1 = spinner1.Position;
                    var position1 = new Vector3((float)(spinner1.R * Math.Sin(thI1)), (float)(spinner1.R * Math.Cos(thI1)), 0) + spinner1.Position;
                    var position2 = new Vector3((float)(spinner2.R * Math.Sin(thI2)), (float)(spinner2.R * Math.Cos(thI2)), 0) + spinner2.Position;


                    // calculate moment arm vector
                    var r = position1 - center1;


                    // calculate force vector
                    var d = position2 - position1; // this vector has the right direction but not the right length.
                                                   // Calculate force now
                    var f1 = MagFieldFromDistance(magnet1.R, magnet1.H, magnet1.Polarity); // field 1
                    var f2 = MagFieldFromDistance(magnet2.R, magnet2.H, magnet2.Polarity); // field 2
                                                                                           // find out the unit vector in this direction first
                    var d_mag = d.Length();
                    var F_mag = (float)(f1 * f2 / Math.Pow(d_mag, 3)); // lets assume that the field varies cubically.
                    var F_u = d / d_mag;
                    var F = F_mag * F_u;

                    if (F_mag > 1000 || F_mag < -1000)
                        ;
                    var tau = Vector3.Cross(r, F);
                    totalTorque = Vector3.One * totalTorque + tau;
                }
            }
            return totalTorque;
        }
        double MagFieldFromDistance(double r, double magnetHeight, bool magnetPolarity) 
        {
            var M = 0.2 * magnetHeight;
            var mu_o = 5e-5;
            var B_r = (mu_o * M) / (4 * Math.PI * Math.Pow(r, 3)); // Mag field strength at this distance.
            if (magnetPolarity)
                return B_r;
            else
                return -B_r;
        }
    }
}
