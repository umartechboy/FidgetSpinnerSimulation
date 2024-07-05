function drawSpinner(spinner)
    hold on;
    
    thD = 2 * pi / length(spinner.Magnets);
    for ii = 1:length(spinner.Magnets)
        thI = spinner.th + thD * (ii - 1);
        c = spinner.R * [cos(thI), sin(thI), 0] + spinner.Position;
        c = c(1:2);
        col = 'b';
        if (spinner.Magnets(ii).Polarity)
            col = 'r';
        end
        viscircles(c, spinner.Magnets(ii).R, 'EdgeColor', col);
        plot([c(1) , spinner.Position(1)], [c(2) , spinner.Position(2)], 'black', 'LineWidth', 2);
    end
    grid on;
end