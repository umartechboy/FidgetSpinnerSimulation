%% Calculates the torque on spinner 1 by spinner 1
function totalTorque = calculateTorque(spinner1, spinner2)

totalTorque = 0;
thD1 = 2 * pi / length(spinner1.Magnets); % angle between magnets on spinner 1
thD2 = 2 * pi / length(spinner2.Magnets); % angle between magnets on spinner 2
for is1 = 1:length(spinner1.Magnets) % itereate through spinner 1 magnets
    for is2 = 1:length(spinner2.Magnets) % itereate through spinner 2 magnets 
        thI1 = spinner1.th + thD1 * (is1 - 1); % angle of magnet ith on spinner 1 in global f.o.r
        thI2 = spinner2.th + thD2 * (is2 - 1); % angle of magnet ith on spinner 2 in global f.o.r
        
        % for simplicity, lets extract the magnets
        magnet1 = spinner1.Magnets(is1);
        magnet2 = spinner1.Magnets(is2);
        
        % to calculate torque, we need the center of spinner and positions
        % of the two magents
        center1 = spinner1.Position;
        position1 = [spinner1.R * sin(thI1), spinner1.R * cos(thI1), 0] + spinner1.Position;
        position2 = [spinner2.R * sin(thI2), spinner2.R * cos(thI2), 0] + spinner2.Position;
        
        % calculate moment arm vector
        r = position1 - center1;
        
        % calculate force vector 
        d = position2 - position1; % this vector has the right direction but not the right length. 
        % Calculate force now        
        f1 = MagFieldFromDistance(magnet1.R, magnet1.H, magnet1.Polarity); % field 1
        f2 = MagFieldFromDistance(magnet2.R, magnet2.H, magnet2.Polarity); % field 2
        % find out the unit vector in this direction first
        d_mag = sqrt(sum(d.^2));
        F_mag = f1 * f2 / d_mag ^ 3; % lets assume that the field varies cubically. 
        F_u = d / d_mag;
        F = F_mag * F_u;
        
        if (F_mag > 1000 || F_mag < -1000)
            ff= 0;
        end
        tau = cross(r, F);
        totalTorque = totalTorque + tau;
    end
end
end