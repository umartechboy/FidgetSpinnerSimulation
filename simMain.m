close all;
clear;
clc;

centerToCenter_mm = 100;
spinners = [...
    makeSpinner(4, 40/1000, [centerToCenter_mm * 0, 0]), ...
    makeSpinner(4, 40/1000, [centerToCenter_mm * 1, 0] / 1000)...
    makeSpinner(4, 40/1000, [centerToCenter_mm * 2, 0] / 1000)...
    ];
spinners(1).IsPowered = true;
spinners(1).w = 0.2;

fMain = figure; 
grid on; axis equal;
fTrq = figure; 
fNTrq = figure; 
fFrc = figure;
fAcc = figure; 
fVel = figure; 
fDisp = figure;
cascade;
% calculate torque on all the spinners

times = [,];
torques = [,];
netTorques = [,];
frictions = [,];
accelerations = [,];
velocities = [,];
displacements = [,];

dt = 0.0001; % ti
tic;
lastPlotAt = tic;
for t = 0:dt:30
    torques(1, end + 1) = 0;
    netTorques(1, end + 1) = 0;
    frictions(1, end + 1) = 0;
    accelerations(1, end + 1) = 0;
    velocities(1, end + 1) = 0;
    displacements(1, end + 1) = 0;
    for ii=1:length(spinners)    
        tauOnii = 0;
        for jj=1:length(spinners)
            if (ii == jj) % same instances
                continue;
            end
            if (spinners(ii).IsPowered)
                continue;
            end
            tauOnii = tauOnii + calculateTorque(spinners(ii), spinners(jj));
        end
        
        if (spinners(ii).IsPowered)  
            % w is fixed                      
            tauF = 0;
            alpha = 0;
            torques(ii, end) = 0;
            netTorques(ii, end) = 0;
        else
            % calculate the torque due to friction
            tauF = -spinners(ii).w * spinners(ii).B;
            % calculate angular velocities
            alpha = (tauOnii(3) + tauF) / spinners(ii).I; %% We only need the vertical component of rotation
            spinners(ii).w = spinners(ii).w + alpha * dt; 
            torques(ii, end) = tauOnii(3);
            netTorques(ii, end) = tauOnii(3) + tauF;
        end
        frictions(ii, end) = tauF;
        accelerations(ii, end) = alpha;
        velocities(ii, end) = spinners(ii).w;
    end
    for ii=1:length(spinners)
        % we have the velocities now, calculate final positions
        spinners(ii).th = spinners(ii).th + spinners(ii).w * dt; 
        displacements(ii, end) = spinners(ii).th;
    end
    times(end + 1) = t;
    
    if (t < toc && (lastPlotAt - toc) < 0.5) % Simulation still needs to progress)
        continue;
    end
    
    % positions have been updated. Draw Now;
    figure(fMain);
    clf; % clear existing
    hold on;
    for ii=1:length(spinners)
        drawSpinner(spinners(ii));
    end
    % make sure the limit stays the same
    plot(-0.05,0)
    plot(0.15,0)
    title(['t = ', num2str(t)]);
    axis equal
    
    % plot data    
    
    figure(fTrq); % torques
    clf;
    for ii=1:length(spinners)
        plot(times, torques(ii, :)); 
        hold on;
    end    
    title('Torques')
    
    figure(fNTrq); % net torques
    clf;
    for ii=1:length(spinners)
        plot(times, netTorques(ii, :)); 
        hold on;
    end    
    title('Net Torques')
    
    
    figure(fFrc); % frictions
    clf;
    for ii=1:length(spinners)
        plot(times, frictions(ii, :)); 
        hold on;
    end    
    title('Frictions')
    
    figure(fAcc); % accelerations
    clf;
    for ii=1:length(spinners)
        plot(times, accelerations(ii, :)); 
        hold on;
    end    
    title('Accelerations')
    
    figure(fVel); % Velocities
    clf;
    for ii=1:length(spinners)
        plot(times, velocities(ii, :)); 
        hold on;
    end
    title('Velocities')
    
    figure(fDisp); % Dispalcements
    clf;
    for ii=1:length(spinners)
        plot(times, displacements(ii, :)); 
        hold on;
    end
    title('Dispalcements')
    
    pause(0.001);
end
