function spinner = makeSpinner(nMagnets, r, position)
spinner = [];
if length(position) == 2
    position(3) = 0; % Fix  Z
end
magnets = makeMagnet(0); % will be replaced
spinner.N = nMagnets;
thD = 2 * pi / spinner.N;
for ii = 0:(spinner.N - 1)
    thI = thD * ii;
    magnets(ii + 1) = makeMagnet(1);
    magnets(ii + 1).Position = r * [cos(thI); sin(thI)];
end
spinner.I = 2; % random value. Should also integrate I from magnets
spinner.B = 200; % random Friction
spinner.Magnets = magnets;
spinner.Position = position; % 3D position
spinner.R = r; % Radius. Also used to initialize magnets
spinner.w = 0; % rotational velocity
spinner.th = 0; % rotational position
spinner.IsPowered = false;
end