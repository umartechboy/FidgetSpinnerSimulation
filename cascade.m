function cascade
%CASCADE Cascade existing figures so that they don't directly overlap.
%   CASCADE takes and returns no arguments.  This function will cascade as
%   many figures as will fit the height/width of the screen.  If there are
%   more figures than can cascade in a screen, those additional figures are
%   left in their original position.
%
%   Author: Isaac Noh
%   Copyright The MathWorks, Inc.
%   November 2007

% Find Existing Figures
figs = findobj(0,'Type','figure'); 
figs = sort(figs);

% Size of Entire Screen
ss = get(0,'ScreenSize'); 

set(figs,'Units','pixels')

padX = 5;
padY = 50;
deltaX = ss(3) / 3;
deltaY = ss(4) / 2;
curY = 0;
curX = ss(3) - deltaX;
for n = 1:length(figs)
    set(figs(n),'Position',[curX curY deltaX-padX, deltaY-padY]);
    curY = curY + deltaY;
    if (curY + deltaY > ss(4))
        curY = 0;
        curX = curX - deltaX;
    end
    figure(figs(n));
end