function out = MagFieldFromDistance(r, magnetHeight, magnetPolarity)
    M = 0.2 * magnetHeight;
    mu_o = 1e-7;
    B_r = (mu_o * M) / (4 * pi * r .^ 3); % Mag field strength at this distance.
    if (magnetPolarity == 1)
        out = B_r;
    else
        out = -B_r;
    end
end