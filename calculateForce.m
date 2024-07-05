function LoA = calculateForce(magnet1, magnet2)
f1 = MagFieldFromDistance(magnet1.R, magnet1.H, magnet1.Polarity);
f2 = MagFieldFromDistance(magnet2.R, magnet2.H, magnet2.Polarity);
d = sqrt((magnet2.Position.x - magnet1.Position.x) ^2 + (magnet2.Position.y - magnet1.Position.y) ^ 2);
F = f1 * f2 / d ^ 3; % lets assume that the field varies cubically
LoA = [];
LoA.x = magnet1.Position.x;
LoA.y = magnet1.Position.y;
LoA.th = atan2(magnet2.Position.y - magnet1.Position.y, magnet2.Position.x - magnet1.Position.x);
LoA.F = F * [cos(LoA.th); sin(LoA.th)];
end