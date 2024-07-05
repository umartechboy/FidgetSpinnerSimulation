function spinner = makeSpinner(nMagnets, r, position, polarity)
if(nargin == 3)
    polarity = true;
end
while(length(polarity) < nMagnets)
    polarity(end + 1) = polarity(1);
end
spinner = [];
if length(position) == 2
    position(3) = 0; % Fix  Z
end
magnets = makeMagnet(true); % will be replaced. Its only here to make struct
spinner.N = nMagnets;
thD = 2 * pi / spinner.N;
spinner.I = 2; % random value for the spinner frame

for ii = 0:(spinner.N - 1)
    thI = thD * ii;
    magnets(ii + 1) = makeMagnet(polarity(ii + 1)); 
    magnets(ii + 1).Position = r * [cos(thI); sin(thI)];
    spinner.I = magnets(ii + 1).Mass * r^2; % parallel axis theorem
end
spinner.B = 200; % random Friction
spinner.Magnets = magnets;
spinner.Position = position; % 3D position
spinner.R = r; % Radius. Also used to initialize magnets
spinner.w = 0; % rotational velocity
spinner.th = 0; % rotational position
spinner.IsPowered = false;
end