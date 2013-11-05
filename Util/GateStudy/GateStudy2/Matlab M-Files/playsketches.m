function playsketches(files)

for j = 1:length(files)
    sketches = importdata(char(files(j)));
    % get max/min x and y's for the axes of the plot.
    maxX = (-1)*Inf();
    maxY = (-1)*Inf();
    minX = Inf();
    minY = Inf();
    for i = 1: length(sketches.data)
       if sketches.data(i,1) < minX
           minX = sketches.data(i,1);
       end
       if sketches.data(i,1) > maxX
           maxX = sketches.data(i,1);
       end
       if sketches.data(i,2) < minY
           minY = sketches.data(i,2);
       end
       if sketches.data(i,2) > maxY
           maxY = sketches.data(i,2);
       end
    end
    %maxX = 1.25*maxX;
    %minX = minX-.25*maxX;
    %maxY = 1.25*maxY;
    %minY = minY-.25*maxY;

    % Find all the different sketches in the file
    sketchIndex = 1;
    for i = 2:length(sketches.data)
       if ~strcmp(sketches.textdata(i,1), sketches.textdata(sketchIndex(length(sketchIndex)),1))
           sketchIndex = [sketchIndex;i];
       end
    end
    sketchIndex = [sketchIndex;length(sketches.data)];

    % Plot each sketch and save it as a seperate file
    for k = 1:length(sketchIndex)-1
        %figure;
        x = zeros(sketchIndex(k+1)-sketchIndex(k),1);
        y = zeros(sketchIndex(k+1)-sketchIndex(k),1);
        %f = zeros(1,sketchIndex(k+1)-sketchIndex(k)+1);
        count = 1;
        numpoints = sketchIndex(k+1)-sketchIndex(k);
        if numpoints < 2500
            for i = sketchIndex(k):3:sketchIndex(k+1)-1
                %if strcmp(sketches.textdata(i,1),sketches.textdata(sketchIndex(k),1))
                    x(count) = sketches.data(i,1);
                    y(count) = (-1)*sketches.data(i,2);
                    plot(x,y,'+')
                    axis([(.9)*minX (1.1)*maxX (-1.1)*maxY (-.9)*minY]);
                    f(count) = getframe(gcf);
                    count = count+1;
                %end
            end
        else
            for i = sketchIndex(k):5:sketchIndex(k+1)-1
                %if strcmp(sketches.textdata(i,1),sketches.textdata(sketchIndex(k),1))
                    x(count) = sketches.data(i,1);
                    y(count) = (-1)*sketches.data(i,2);
                    plot(x,y,'+')
                    axis([(.9)*minX (1.1)*maxX (-1.1)*maxY (-.9)*minY]);
                    f(count) = getframe(gcf);
                    count = count+1;
                %end
            end
        end
        movie2avi(f,['User',int2str(j),'_Sketch',int2str(k)],'fps',15);
        clear f x y;
    end
    %clear sketches;
end
    