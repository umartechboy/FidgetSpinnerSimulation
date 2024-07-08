function magnet = makeMagnet(polarity)
    magnet.Position = [0; 0];
    magnet.R = 15 / 2 / 1000;
    magnet.H = 6 / 1000;
    magnet.Polarity = polarity;
    magnet.Mass = 10 / 1000; % kg
end