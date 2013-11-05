function gatestats(file)

% import the data into a vector (actually 2 vectors 
% gatedata.textdata and gatedata.data)
% gatedata = importdata('GateStats.csv');


% 
%for n=1:length(files)
    %length(file)
    data = importdata(char(file));%(1,n)));
    rWirePressure = [];
    rAndPressure = [];
    rOrPressure = [];
    rXorPressure = [];
    rNandPressure = [];
    rNorPressure = [];
    rNotPressure = [];
    eWirePressure = [];
    eAndPressure = [];
    eOrPressure = [];
    eXorPressure = [];
    eNandPressure = [];
    eNorPressure = [];
    eNotPressure = [];
    cWirePressure = [];
    cAndPressure = [];
    cOrPressure = [];
    cXorPressure = [];
    cNandPressure = [];
    cNorPressure = [];
    cNotPressure = [];
    
    rWireWidth = [];
    rAndWidth = [];
    rOrWidth = [];
    rXorWidth = [];
    rNandWidth = [];
    rNorWidth = [];
    rNotWidth = [];
    eWireWidth = [];
    eAndWidth = [];
    eOrWidth = [];
    eXorWidth = [];
    eNandWidth = [];
    eNorWidth = [];
    eNotWidth = [];
    cWireWidth = [];
    cAndWidth = [];
    cOrWidth = [];
    cXorWidth = [];
    cNandWidth = [];
    cNorWidth = [];
    cNotWidth = [];
    
    rWireHeight = [];
    rAndHeight = [];
    rOrHeight = [];
    rXorHeight = [];
    rNandHeight = [];
    rNorHeight = [];
    rNotHeight = [];
    eWireHeight = [];
    eAndHeight = [];
    eOrHeight = [];
    eXorHeight = [];
    eNandHeight = [];
    eNorHeight = [];
    eNotHeight = [];
    cWireHeight = [];
    cAndHeight = [];
    cOrHeight = [];
    cXorHeight = [];
    cNandHeight = [];
    cNorHeight = [];
    cNotHeight = [];
    
    rWireCurvature = [];
    rAndCurvature = [];
    rOrCurvature = [];
    rXorCurvature = [];
    rNandCurvature = [];
    rNorCurvature = [];
    rNotCurvature = [];
    eWireCurvature = [];
    eAndCurvature = [];
    eOrCurvature = [];
    eXorCurvature = [];
    eNandCurvature = [];
    eNorCurvature = [];
    eNotCurvature = [];
    cWireCurvature = [];
    cAndCurvature = [];
    cOrCurvature = [];
    cXorCurvature = [];
    cNandCurvature = [];
    cNorCurvature = [];
    cNotCurvature = [];
    
    rWireNumPoints = [];
    rAndNumPoints = [];
    rOrNumPoints = [];
    rXorNumPoints = [];
    rNandNumPoints = [];
    rNorNumPoints = [];
    rNotNumPoints = [];
    eWireNumPoints = [];
    eAndNumPoints = [];
    eOrNumPoints = [];
    eXorNumPoints = [];
    eNandNumPoints = [];
    eNorNumPoints = [];
    eNotNumPoints = [];
    cWireNumPoints = [];
    cAndNumPoints = [];
    cOrNumPoints = [];
    cXorNumPoints = [];
    cNandNumPoints = [];
    cNorNumPoints = [];
    cNotNumPoints = [];
    
    rWireMinDist = [];
    rAndMinDist = [];
    rOrMinDist = [];
    rXorMinDist = [];
    rNandMinDist = [];
    rNorMinDist = [];
    rNotMinDist = [];
    eWireMinDist = [];
    eAndMinDist = [];
    eOrMinDist = [];
    eXorMinDist = [];
    eNandMinDist = [];
    eNorMinDist = [];
    eNotMinDist = [];
    cWireMinDist = [];
    cAndMinDist = [];
    cOrMinDist = [];
    cXorMinDist = [];
    cNandMinDist = [];
    cNorMinDist = [];
    cNotMinDist = [];
    
    rConsecutiveShapes = 0;
    eConsecutiveShapes = 0;
    cConsecutiveShapes = 0;
    rTotalShapes = 0;
    eTotalShapes = 0;
    cTotalShapes = 0;
    
    
    for i=1:length(data.textdata)
        
        if strcmp(data.textdata(i,3),'AND')
            if strcmp(data.textdata(i,6),'AND')
                if i == 1 || ~strcmp(data.textdata(i,4),data.textdata(i-1,4))
                    rAndWidth = [rAndWidth data.data(i,2)];
                    rAndHeight = [rAndHeight data.data(i,3)];
                    rAndMinDist = [rAndMinDist data.data(i,8)];
                    if strcmp(data.textdata(i,9), 'TRUE')
                        rConsecutiveShapes = rConsecutiveShapes + 1;
                    end
                    rTotalShapes = rTotalShapes + 1;
                end
                rAndPressure = [rAndPressure data.data(i,1)];
                rAndCurvature = [rAndCurvature data.data(i,4)];
                rAndNumPoints = [rAndNumPoints data.data(i,7)];
            else
                if i == 1 || ~strcmp(data.textdata(i,4),data.textdata(i-1,4))
                    rWireWidth = [rWireWidth data.data(i,2)];
                    rWireHeight = [rWireHeight data.data(i,3)];
                    rWireMinDist = [rWireMinDist data.data(i,8)];
                    if strcmp(data.textdata(i,9), 'TRUE')
                        rConsecutiveShapes = rConsecutiveShapes + 1;
                    end
                    rTotalShapes = rTotalShapes + 1;
                end
                rWirePressure = [rWirePressure data.data(i,1)];
                rWireCurvature = [rWireCurvature data.data(i,4)];
                rWireNumPoints = [rWireNumPoints data.data(i,7)];
            end
        elseif strcmp(data.textdata(i,3),'OR')
            if strcmp(data.textdata(i,6),'OR')
                if i == 1 || ~strcmp(data.textdata(i,4),data.textdata(i-1,4))
                    rOrWidth = [rOrWidth data.data(i,2)];
                    rOrHeight = [rOrHeight data.data(i,3)];
                    rOrMinDist = [rOrMinDist data.data(i,8)];
                    if strcmp(data.textdata(i,9), 'TRUE')
                        rConsecutiveShapes = rConsecutiveShapes + 1;
                    end
                    rTotalShapes = rTotalShapes + 1;
                end
                rOrPressure = [rOrPressure data.data(i,1)];
                rOrCurvature = [rOrCurvature data.data(i,4)];
                rOrNumPoints = [rOrNumPoints data.data(i,7)];
            else
                if i == 1 || ~strcmp(data.textdata(i,4),data.textdata(i-1,4))
                    rWireWidth = [rWireWidth data.data(i,2)];
                    rWireHeight = [rWireHeight data.data(i,3)];
                    rWireMinDist = [rWireMinDist data.data(i,8)];
                    if strcmp(data.textdata(i,9), 'TRUE')
                        rConsecutiveShapes = rConsecutiveShapes + 1;
                    end
                    rTotalShapes = rTotalShapes + 1;
                end
                rWirePressure = [rWirePressure data.data(i,1)];
                rWireCurvature = [rWireCurvature data.data(i,4)];
                rWireNumPoints = [rWireNumPoints data.data(i,7)];
            end
        elseif strcmp(data.textdata(i,3),'XOR')
            if strcmp(data.textdata(i,6),'XOR')
                if i == 1 || ~strcmp(data.textdata(i,4),data.textdata(i-1,4))
                    rXorWidth = [rXorWidth data.data(i,2)];
                    rXorHeight = [rXorHeight data.data(i,3)];
                    rXorMinDist = [rXorMinDist data.data(i,8)];
                    if strcmp(data.textdata(i,9), 'TRUE')
                        rConsecutiveShapes = rConsecutiveShapes + 1;
                    end
                    rTotalShapes = rTotalShapes + 1;
                end
                rXorPressure = [rXorPressure data.data(i,1)];
                rXorCurvature = [rXorCurvature data.data(i,4)];
                rXorNumPoints = [rXorNumPoints data.data(i,7)];
            else
                if i == 1 || ~strcmp(data.textdata(i,4),data.textdata(i-1,4))
                    rWireWidth = [rWireWidth data.data(i,2)];
                    rWireHeight = [rWireHeight data.data(i,3)];
                    rWireMinDist = [rWireMinDist data.data(i,8)];
                    if strcmp(data.textdata(i,9), 'TRUE')
                        rConsecutiveShapes = rConsecutiveShapes + 1;
                    end
                    rTotalShapes = rTotalShapes + 1;
                end
                rWirePressure = [rWirePressure data.data(i,1)];
                rWireCurvature = [rWireCurvature data.data(i,4)];
                rWireNumPoints = [rWireNumPoints data.data(i,7)];
            end
        elseif strcmp(data.textdata(i,3),'NAND')
            if strcmp(data.textdata(i,6),'NAND')
                if i == 1 || ~strcmp(data.textdata(i,4),data.textdata(i-1,4))
                    rNandWidth = [rNandWidth data.data(i,2)];
                    rNandHeight = [rNandHeight data.data(i,3)];
                    rNandMinDist = [rNandMinDist data.data(i,8)];
                    if strcmp(data.textdata(i,9), 'TRUE')
                        rConsecutiveShapes = rConsecutiveShapes + 1;
                    end
                    rTotalShapes = rTotalShapes + 1;
                end
                rNandPressure = [rNandPressure data.data(i,1)];
                rNandCurvature = [rNandCurvature data.data(i,4)];
                rNandNumPoints = [rNandNumPoints data.data(i,7)];
            else
                if i == 1 || ~strcmp(data.textdata(i,4),data.textdata(i-1,4))
                    rWireWidth = [rWireWidth data.data(i,2)];
                    rWireHeight = [rWireHeight data.data(i,3)];
                    rWireMinDist = [rWireMinDist data.data(i,8)];
                    if strcmp(data.textdata(i,9), 'TRUE')
                        rConsecutiveShapes = rConsecutiveShapes + 1;
                    end
                    rTotalShapes = rTotalShapes + 1;
                end
                rWirePressure = [rWirePressure data.data(i,1)];
                rWireCurvature = [rWireCurvature data.data(i,4)];
                rWireNumPoints = [rWireNumPoints data.data(i,7)];
            end
        elseif strcmp(data.textdata(i,3),'NOR')
            if strcmp(data.textdata(i,6),'NOR')
                if i == 1 || ~strcmp(data.textdata(i,4),data.textdata(i-1,4))
                    rNorWidth = [rNorWidth data.data(i,2)];
                    rNorHeight = [rNorHeight data.data(i,3)];
                    rNorMinDist = [rNorMinDist data.data(i,8)];
                    if strcmp(data.textdata(i,9), 'TRUE')
                        rConsecutiveShapes = rConsecutiveShapes + 1;
                    end
                    rTotalShapes = rTotalShapes + 1;
                end
                rNorPressure = [rNorPressure data.data(i,1)];
                rNorCurvature = [rNorCurvature data.data(i,4)];
                rNorNumPoints = [rNorNumPoints data.data(i,7)];
            else
                if i == 1 || ~strcmp(data.textdata(i,4),data.textdata(i-1,4))
                    rWireWidth = [rWireWidth data.data(i,2)];
                    rWireHeight = [rWireHeight data.data(i,3)];
                    rWireMinDist = [rWireMinDist data.data(i,8)];
                    if strcmp(data.textdata(i,9), 'TRUE')
                        rConsecutiveShapes = rConsecutiveShapes + 1;
                    end
                    rTotalShapes = rTotalShapes + 1;
                end
                rWirePressure = [rWirePressure data.data(i,1)];
                rWireCurvature = [rWireCurvature data.data(i,4)];
                rWireNumPoints = [rWireNumPoints data.data(i,7)];
            end
        elseif strcmp(data.textdata(i,3),'NOT')
            if strcmp(data.textdata(i,6),'NOT')
                if i == 1 || ~strcmp(data.textdata(i,4),data.textdata(i-1,4))
                    rNotWidth = [rNotWidth data.data(i,2)];
                    rNotHeight = [rNotHeight data.data(i,3)];
                    rNotMinDist = [rNotMinDist data.data(i,8)];
                    if strcmp(data.textdata(i,9), 'TRUE')
                        rConsecutiveShapes = rConsecutiveShapes + 1;
                    end
                    rTotalShapes = rTotalShapes + 1;
                end
                rNotPressure = [rNotPressure data.data(i,1)];
                rNotCurvature = [rNotCurvature data.data(i,4)];
                rNotNumPoints = [rNotNumPoints data.data(i,7)];
            else
                if i == 1 || ~strcmp(data.textdata(i,4),data.textdata(i-1,4))
                    rWireWidth = [rWireWidth data.data(i,2)];
                    rWireHeight = [rWireHeight data.data(i,3)];
                    rWireMinDist = [rWireMinDist data.data(i,8)];
                    if strcmp(data.textdata(i,9), 'TRUE')
                        rConsecutiveShapes = rConsecutiveShapes + 1;
                    end
                    rTotalShapes = rTotalShapes + 1;
                end
                rWirePressure = [rWirePressure data.data(i,1)];
                rWireCurvature = [rWireCurvature data.data(i,4)];
                rWireNumPoints = [rWireNumPoints data.data(i,7)];
            end
        elseif strcmp(data.textdata(i,3),'EQ1')||strcmp(data.textdata(i,3),'EQ2')
            switch char(data.textdata(i,6))
                case 'Wire'
                    
                    if i == 1 || ~strcmp(data.textdata(i,4),data.textdata(i-1,4))
                        eWireWidth = [eWireWidth data.data(i,2)];
                        eWireHeight = [eWireHeight data.data(i,3)];
                        eWireMinDist = [eWireMinDist data.data(i,8)];
                        if strcmp(data.textdata(i,9), 'TRUE')
                            eConsecutiveShapes = eConsecutiveShapes + 1;
                        end
                        eTotalShapes = eTotalShapes + 1;
                    end
                    eWirePressure = [eWirePressure data.data(i,1)];
                    eWireCurvature = [eWireCurvature data.data(i,4)];
                    eWireNumPoints = [eWireNumPoints data.data(i,7)];

                case 'AND'
                    
                    if i == 1 || ~strcmp(data.textdata(i,4),data.textdata(i-1,4))
                        eAndWidth = [eAndWidth data.data(i,2)];
                        eAndHeight = [eAndHeight data.data(i,3)];
                        eAndMinDist = [eAndMinDist data.data(i,8)];
                        if strcmp(data.textdata(i,9), 'TRUE')
                            eConsecutiveShapes = eConsecutiveShapes + 1;
                        end
                        eTotalShapes = eTotalShapes + 1;
                    end
                    eAndPressure = [eAndPressure data.data(i,1)];
                    eAndCurvature = [eAndCurvature data.data(i,4)];
                    eAndNumPoints = [eAndNumPoints data.data(i,7)];

                case 'OR'
                    
                    if i == 1 || ~strcmp(data.textdata(i,4),data.textdata(i-1,4))
                        eOrWidth = [eOrWidth data.data(i,2)];
                        eOrHeight = [eOrHeight data.data(i,3)];
                        eOrMinDist = [eOrMinDist data.data(i,8)];
                        if strcmp(data.textdata(i,9), 'TRUE')
                            eConsecutiveShapes = eConsecutiveShapes + 1;
                        end
                        eTotalShapes = eTotalShapes + 1;
                    end
                    eOrPressure = [eOrPressure data.data(i,1)];
                    eOrCurvature = [eOrCurvature data.data(i,4)];
                    eOrNumPoints = [eOrNumPoints data.data(i,7)];

                case 'XOR'
                    
                    if i == 1 || ~strcmp(data.textdata(i,4),data.textdata(i-1,4))
                        eXorWidth = [eXorWidth data.data(i,2)];
                        eXorHeight = [eXorHeight data.data(i,3)];
                        eXorMinDist = [eXorMinDist data.data(i,8)];
                        if strcmp(data.textdata(i,9), 'TRUE')
                            eConsecutiveShapes = eConsecutiveShapes + 1;
                        end
                        eTotalShapes = eTotalShapes + 1;
                    end
                    eXorPressure = [eXorPressure data.data(i,1)];
                    eXorCurvature = [eXorCurvature data.data(i,4)];
                    eXorNumPoints = [eXorNumPoints data.data(i,7)];

                case 'NAND'
                    
                    if i == 1 || ~strcmp(data.textdata(i,4),data.textdata(i-1,4))
                        eNandWidth = [eNandWidth data.data(i,2)];
                        eNandHeight = [eNandHeight data.data(i,3)];
                        eNandMinDist = [eNandMinDist data.data(i,8)];
                        if strcmp(data.textdata(i,9), 'TRUE')
                            eConsecutiveShapes = eConsecutiveShapes + 1;
                        end
                        eTotalShapes = eTotalShapes + 1;
                    end
                    eNandPressure = [eNandPressure data.data(i,1)];
                    eNandCurvature = [eNandCurvature data.data(i,4)];
                    eNandNumPoints = [eNandNumPoints data.data(i,7)];

                case 'NOR'
                    
                    if i == 1 || ~strcmp(data.textdata(i,4),data.textdata(i-1,4))
                        eNorWidth = [eNorWidth data.data(i,2)];
                        eNorHeight = [eNorHeight data.data(i,3)];
                        eNorMinDist = [eNorMinDist data.data(i,8)];
                        if strcmp(data.textdata(i,9), 'TRUE')
                            eConsecutiveShapes = eConsecutiveShapes + 1;
                        end
                        eTotalShapes = eTotalShapes + 1;
                    end
                    eNorPressure = [eNorPressure data.data(i,1)];
                    eNorCurvature = [eNorCurvature data.data(i,4)];
                    eNorNumPoints = [eNorNumPoints data.data(i,7)];

                case 'NOT'
                    
                    if i == 1 || ~strcmp(data.textdata(i,4),data.textdata(i-1,4))
                        eNotWidth = [eNotWidth data.data(i,2)];
                        eNotHeight = [eNotHeight data.data(i,3)];
                        eNotMinDist = [eNotMinDist data.data(i,8)];
                        if strcmp(data.textdata(i,9), 'TRUE')
                            eConsecutiveShapes = eConsecutiveShapes + 1;
                        end
                        eTotalShapes = eTotalShapes + 1;
                    end
                    eNotPressure = [eNotPressure data.data(i,1)];
                    eNotCurvature = [eNotCurvature data.data(i,4)];
                    eNotNumPoints = [eNotNumPoints data.data(i,7)];
                    
            end
            
        elseif strcmp(data.textdata(i,3),'COPY1')||strcmp(data.textdata(i,3),'COPY2')
            
            switch char(data.textdata(i,6))
                case 'Wire'
                    
                    if i == 1 || ~strcmp(data.textdata(i,4),data.textdata(i-1,4))
                        cWireWidth = [cWireWidth data.data(i,2)];
                        cWireHeight = [cWireHeight data.data(i,3)];
                        cWireMinDist = [cWireMinDist data.data(i,8)];
                        if strcmp(data.textdata(i,9), 'TRUE')
                            cConsecutiveShapes = cConsecutiveShapes + 1;
                        end
                        cTotalShapes = cTotalShapes + 1;
                    end
                    cWirePressure = [cWirePressure data.data(i,1)];
                    cWireCurvature = [cWireCurvature data.data(i,4)];
                    cWireNumPoints = [cWireNumPoints data.data(i,7)];

                case 'AND'
                    
                    if i == 1 || ~strcmp(data.textdata(i,4),data.textdata(i-1,4))
                        cAndWidth = [cAndWidth data.data(i,2)];
                        cAndHeight = [cAndHeight data.data(i,3)];
                        cAndMinDist = [cAndMinDist data.data(i,8)];
                        if strcmp(data.textdata(i,9), 'TRUE')
                            cConsecutiveShapes = cConsecutiveShapes + 1;
                        end
                        cTotalShapes = cTotalShapes + 1;
                    end
                    cAndPressure = [cAndPressure data.data(i,1)];
                    cAndCurvature = [cAndCurvature data.data(i,4)];
                    cAndNumPoints = [cAndNumPoints data.data(i,7)];

                case 'OR'
                    
                    if i == 1 || ~strcmp(data.textdata(i,4),data.textdata(i-1,4))
                        cOrWidth = [cOrWidth data.data(i,2)];
                        cOrHeight = [cOrHeight data.data(i,3)];
                        cOrMinDist = [cOrMinDist data.data(i,8)];
                        if strcmp(data.textdata(i,9), 'TRUE')
                            cConsecutiveShapes = cConsecutiveShapes + 1;
                        end
                        cTotalShapes = cTotalShapes + 1;
                    end
                    cOrPressure = [cOrPressure data.data(i,1)];
                    cOrCurvature = [cOrCurvature data.data(i,4)];
                    cOrNumPoints = [cOrNumPoints data.data(i,7)];

                case 'XOR'
                    
                    if i == 1 || ~strcmp(data.textdata(i,4),data.textdata(i-1,4))
                        cXorWidth = [cXorWidth data.data(i,2)];
                        cXorHeight = [cXorHeight data.data(i,3)];
                        cXorMinDist = [cXorMinDist data.data(i,8)];
                        if strcmp(data.textdata(i,9), 'TRUE')
                            cConsecutiveShapes = cConsecutiveShapes + 1;
                        end
                        cTotalShapes = cTotalShapes + 1;
                    end
                    cXorPressure = [cXorPressure data.data(i,1)];
                    cXorCurvature = [cXorCurvature data.data(i,4)];
                    cXorNumPoints = [cXorNumPoints data.data(i,7)];

                case 'NAND'
                    
                    if i == 1 || ~strcmp(data.textdata(i,4),data.textdata(i-1,4))
                        cNandWidth = [cNandWidth data.data(i,2)];
                        cNandHeight = [cNandHeight data.data(i,3)];
                        cNandMinDist = [cNandMinDist data.data(i,8)];
                        if strcmp(data.textdata(i,9), 'TRUE')
                            cConsecutiveShapes = cConsecutiveShapes + 1;
                        end
                        cTotalShapes = cTotalShapes + 1;
                    end
                    cNandPressure = [cNandPressure data.data(i,1)];
                    cNandCurvature = [cNandCurvature data.data(i,4)];
                    cNandNumPoints = [cNandNumPoints data.data(i,7)];

                case 'NOR'
                    
                    if i == 1 || ~strcmp(data.textdata(i,4),data.textdata(i-1,4))
                        cNorWidth = [cNorWidth data.data(i,2)];
                        cNorHeight = [cNorHeight data.data(i,3)];
                        cNorMinDist = [cNorMinDist data.data(i,8)];
                        if strcmp(data.textdata(i,9), 'TRUE')
                            cConsecutiveShapes = cConsecutiveShapes + 1;
                        end
                        cTotalShapes = cTotalShapes + 1;
                    end
                    cNorPressure = [cNorPressure data.data(i,1)];
                    cNorCurvature = [cNorCurvature data.data(i,4)];
                    cNorNumPoints = [cNorNumPoints data.data(i,7)];

                case 'NOT'
                    
                    if i == 1 || ~strcmp(data.textdata(i,4),data.textdata(i-1,4))
                        cNotWidth = [cNotWidth data.data(i,2)];
                        cNotHeight = [cNotHeight data.data(i,3)];
                        cNotMinDist = [cNotMinDist data.data(i,8)];
                        if strcmp(data.textdata(i,9), 'TRUE')
                            cConsecutiveShapes = cConsecutiveShapes + 1;
                        end
                        cTotalShapes = cTotalShapes + 1;
                    end
                    cNotPressure = [cNotPressure data.data(i,1)];
                    cNotCurvature = [cNotCurvature data.data(i,4)];
                    cNotNumPoints = [cNotNumPoints data.data(i,7)];
                    
            end
            
        end

    end
    
    %amf = sum(rWirePressure)/length(rWirePressure)
    
    avgWirePressure = [sum(rWirePressure)/length(rWirePressure) sum(eWirePressure)/length(eWirePressure) sum(cWirePressure)/length(cWirePressure)];
    avgAndPressure = [sum(rAndPressure)/length(rAndPressure) sum(eAndPressure)/length(eAndPressure) sum(cAndPressure)/length( cAndPressure)];
    avgOrPressure = [sum(rOrPressure)/length(rOrPressure) sum(eOrPressure)/length(eOrPressure) sum(cOrPressure)/length(cOrPressure)];
    avgXorPressure = [sum(rXorPressure)/length( rXorPressure) sum(eXorPressure)/length(eXorPressure) sum(cXorPressure)/length(cXorPressure)];
    avgNandPressure = [sum(rNandPressure)/length(rNandPressure) sum(eNandPressure)/length(eNandPressure) sum(cNandPressure)/length( cNandPressure)];
    avgNorPressure = [sum(rNorPressure)/length(rNorPressure) sum(eNorPressure)/length(eNorPressure) sum(cNorPressure)/length(cNorPressure)];
    avgNotPressure = [sum(rNotPressure)/length(rNotPressure) sum(eNotPressure)/length(eNotPressure) sum(cNotPressure)/length(cNotPressure)];
    
    avgWireWidth = [sum(rWireWidth)/length(rWireWidth) sum(eWireWidth)/length(eWireWidth) sum(cWireWidth)/length(cWireWidth)];
    avgAndWidth = [sum(rAndWidth)/length(rAndWidth) sum(eAndWidth)/length(eAndWidth) sum(cAndWidth)/length(cAndWidth)];
    avgOrWidth = [sum(rOrWidth)/length(rOrWidth) sum(eOrWidth)/length(eOrWidth) sum(cOrWidth)/length(cOrWidth)];
    avgXorWidth = [sum(rXorWidth)/length(rXorWidth) sum(eXorWidth)/length(eXorWidth) sum(cXorWidth)/length(cXorWidth)];
    avgNandWidth = [sum(rNandWidth)/length(rNandWidth) sum(eNandWidth)/length(eNandWidth) sum(cNandWidth)/length(cNandWidth)];
    avgNorWidth = [sum(rNorWidth)/length(rNorWidth) sum(eNorWidth)/length(eNorWidth) sum(cNorWidth)/length(cNorWidth)];
    avgNotWidth = [sum(rNotWidth)/length(rNotWidth) sum(eNotWidth)/length(eNotWidth) sum(cNotWidth)/length(cNotWidth)];
    
    
    avgWireHeight =[sum(rWireHeight)/length(rWireHeight) sum(eWireHeight)/length( eWireHeight) sum(cWireHeight)/length(cWireHeight)];
    avgAndHeight = [sum(rAndHeight)/length(rAndHeight) sum(eAndHeight)/length(eAndHeight) sum(cAndHeight)/length(cAndHeight)];
    avgOrHeight = [sum(rOrHeight)/length(rOrHeight) sum(eOrHeight)/length(eOrHeight) sum(cOrHeight)/length(cOrHeight)];
    avgXorHeight = [sum(rXorHeight)/length(rXorHeight) sum(eXorHeight)/length(eXorHeight) sum(cXorHeight)/length(cXorHeight)];
    avgNandHeight = [sum(rNandHeight)/length(rNandHeight) sum(eNandHeight)/length(eNandHeight) sum(cNandHeight)/length(cNandHeight)];
    avgNorHeight = [sum(rNorHeight)/length(rNorHeight) sum(eNorHeight)/length(eNorHeight) sum(cNorHeight)/length(cNorHeight)];
    avgNotHeight = [sum(rNotHeight)/length(rNotHeight) sum(eNotHeight)/length(eNotHeight) sum(cNotHeight)/length(cNotHeight)];
    
    avgWireCurvature =[sum(rWireCurvature)/length(rWireCurvature) sum(eWireCurvature)/length( eWireCurvature) sum(cWireCurvature)/length( cWireCurvature)];
    avgAndCurvature = [sum(rAndCurvature)/length(rAndCurvature) sum(eAndCurvature)/length(eAndCurvature) sum(cAndCurvature)/length(cAndCurvature)];
    avgOrCurvature = [sum(rOrCurvature)/length(rOrCurvature) sum(eOrCurvature)/length(eOrCurvature) sum(cOrCurvature)/length(cOrCurvature)];
    avgXorCurvature = [sum(rXorCurvature)/length(rXorCurvature) sum(eXorCurvature)/length(eXorCurvature) sum(cXorCurvature)/length(cXorCurvature)];
    avgNandCurvature = [sum(rNandCurvature)/length(rNandCurvature) sum(eNandCurvature)/length(eNandCurvature) sum(cNandCurvature)/length(cNandCurvature)];
    avgNorCurvature = [sum(rNorCurvature)/length(rNorCurvature) sum(eNorCurvature)/length(eNorCurvature) sum(cNorCurvature)/length(cNorCurvature)];
    avgNotCurvature = [sum(rNotCurvature)/length(rNotCurvature) sum(eNotCurvature)/length(eNotCurvature) sum(cNotCurvature)/length(cNotCurvature)];
    
    avgWireNumPoints =[sum(rWireNumPoints)/length(rWireNumPoints) sum(eWireNumPoints)/length( eWireNumPoints) sum(cWireNumPoints)/length( cWireNumPoints)];
    avgAndNumPoints = [sum(rAndNumPoints)/length(rAndNumPoints) sum(eAndNumPoints)/length(eAndNumPoints) sum(cAndNumPoints)/length(cAndNumPoints)];
    avgOrNumPoints = [sum(rOrNumPoints)/length(rOrNumPoints) sum(eOrNumPoints)/length(eOrNumPoints) sum(cOrNumPoints)/length(cOrNumPoints)];
    avgXorNumPoints = [sum(rXorNumPoints)/length(rXorNumPoints) sum(eXorNumPoints)/length(eXorNumPoints) sum(cXorNumPoints)/length(cXorNumPoints)];
    avgNandNumPoints = [sum(rNandNumPoints)/length(rNandNumPoints) sum(eNandNumPoints)/length(eNandNumPoints) sum(cNandNumPoints)/length(cNandNumPoints)];
    avgNorNumPoints = [sum(rNorNumPoints)/length(rNorNumPoints) sum(eNorNumPoints)/length(eNorNumPoints) sum(cNorNumPoints)/length(cNorNumPoints)];
    avgNotNumPoints = [sum(rNotNumPoints)/length(rNotNumPoints) sum(eNotNumPoints)/length(eNotNumPoints) sum(cNotNumPoints)/length(cNotNumPoints)];
    
    avgWireMinDist =[sum(rWireMinDist)/length(rWireMinDist) sum(eWireMinDist)/length( eWireMinDist) sum(cWireMinDist)/length( cWireMinDist)];
    avgAndMinDist = [sum(rAndMinDist)/length(rAndMinDist) sum(eAndMinDist)/length(eAndMinDist) sum(cAndMinDist)/length(cAndMinDist)];
    avgOrMinDist = [sum(rOrMinDist)/length(rOrMinDist) sum(eOrMinDist)/length(eOrMinDist) sum(cOrMinDist)/length(cOrMinDist)];
    avgXorMinDist = [sum(rXorMinDist)/length(rXorMinDist) sum(eXorMinDist)/length(eXorMinDist) sum(cXorMinDist)/length(cXorMinDist)];
    avgNandMinDist = [sum(rNandMinDist)/length(rNandMinDist) sum(eNandMinDist)/length(eNandMinDist) sum(cNandMinDist)/length(cNandMinDist)];
    avgNorMinDist = [sum(rNorMinDist)/length(rNorMinDist) sum(eNorMinDist)/length(eNorMinDist) sum(cNorMinDist)/length(cNorMinDist)];
    avgNotMinDist = [sum(rNotMinDist)/length(rNotMinDist) sum(eNotMinDist)/length(eNotMinDist) sum(cNotMinDist)/length(cNotMinDist)];
    
    
   percentageOfConsecutive = [100*(rConsecutiveShapes/length(rTotalShapes)) 100*(eConsecutiveShapes/length(eTotalShapes)) 100*(cConsecutiveShapes/length(cTotalShapes))];
    
    
    totWireData=[avgWirePressure; avgWireWidth; avgWireHeight; avgWireCurvature; avgWireNumPoints; avgWireMinDist];
    totAndData =[avgAndPressure;  avgAndWidth; avgAndHeight; avgAndCurvature; avgAndNumPoints; avgAndMinDist];  
    totOrData =[avgOrPressure; avgOrWidth; avgOrHeight; avgOrCurvature; avgOrNumPoints; avgOrMinDist];
    totXorData =[avgXorPressure; avgXorWidth; avgXorHeight; avgXorCurvature; avgXorNumPoints; avgXorMinDist];
    totNandData=[avgNandPressure; avgNandWidth; avgNandHeight; avgNandCurvature; avgNandNumPoints; avgNandMinDist];
    totNorData=[avgNorPressure; avgNorWidth; avgNorHeight; avgNorCurvature; avgNorNumPoints; avgNorMinDist];
    totNotData =[avgNotPressure; avgNotWidth; avgNotHeight; avgNotCurvature; avgNorNumPoints; avgNotMinDist];
    
    figure
    subplot(1,6,1)
    bar(totWireData(1,:))
    xlabel('Avg Pressure / Stroke')
    subplot(1,6,2)
    bar(totWireData(2,:))
    xlabel('Avg Width / Shape')
    subplot(1,6,3)
    bar(totWireData(3,:))
    xlabel('Avg Height / Shape')
    title('Wire Data','FontWeight','bold')
    subplot(1,6,4)
    bar(totWireData(4,:))
    xlabel('Avg Curvature / Stroke')
    subplot(1,6,5)
    bar(totWireData(5,:))
    xlabel('Avg Points / Stroke')
    subplot(1,6,6)
    bar(totWireData(6,:))
    xlabel('Avg Min Dist to Gate')
    
    figure
    subplot(1,6,1)
    bar(totAndData(1,:))
    xlabel('Avg Pressure / Stroke')
    subplot(1,6,2)
    bar(totAndData(2,:))
    xlabel('Avg Width / Shape')
    subplot(1,6,3)
    bar(totAndData(3,:))
    xlabel('Avg Height / Shape')
    title('And Data','FontWeight','bold')
    subplot(1,6,4)
    bar(totAndData(4,:))
    xlabel('Avg Curvature / Stroke')
    subplot(1,6,5)
    bar(totAndData(5,:))
    xlabel('Avg Points / Stroke')
    subplot(1,6,6)
    bar(totAndData(6,:))
    xlabel('Avg Min Dist to Gate')
    
    figure
    subplot(1,6,1)
    bar(totOrData(1,:))
    xlabel('Avg Pressure / Stroke')
    subplot(1,6,2)
    bar(totOrData(2,:))
    xlabel('Avg Width / Shape')
    subplot(1,6,3)
    bar(totOrData(3,:))
    xlabel('Avg Height / Shape')
    title('Or Data','FontWeight','bold')
    subplot(1,6,4)
    bar(totOrData(4,:))
    xlabel('Avg Curvature / Stroke')
    subplot(1,6,5)
    bar(totOrData(5,:))
    xlabel('Avg Points / Stroke')
    subplot(1,6,6)
    bar(totOrData(6,:))
    xlabel('Avg Min Dist to Gate')
    
    
    figure
    subplot(1,6,1)
    bar(totXorData(1,:))
    xlabel('Avg Pressure / Stroke')
    subplot(1,6,2)
    bar(totXorData(2,:))
    xlabel('Avg Width / Shape')
    subplot(1,6,3)
    bar(totXorData(3,:))
    xlabel('Avg Height / Shape')
    title('Xor Data','FontWeight','bold')
    subplot(1,6,4)
    bar(totXorData(4,:))
    xlabel('Avg Curvature / Stroke')
    subplot(1,6,5)
    bar(totXorData(5,:))
    xlabel('Avg Points / Stroke')
    subplot(1,6,6)
    bar(totXorData(6,:))
    xlabel('Avg Min Dist to Gate')
    
    figure
    subplot(1,6,1)
    bar(totNandData(1,:))
    xlabel('Avg Pressure / Stroke')
    subplot(1,6,2)
    bar(totNandData(2,:))
    xlabel('Avg Width / Shape')
    subplot(1,6,3)
    bar(totNandData(3,:))
    xlabel('Avg Height / Shape')
    title('Nand Data','FontWeight','bold')
    subplot(1,6,4)
    bar(totNandData(4,:))
    xlabel('Avg Curvature / Stroke')
    subplot(1,6,5)
    bar(totNandData(5,:))
    xlabel('Avg Points / Stroke')
    subplot(1,6,6)
    bar(totNandData(6,:))
    xlabel('Avg Min Dist to Gate')
    
    figure
    subplot(1,6,1)
    bar(totNorData(1,:))
    xlabel('Avg Pressure / Stroke')
    subplot(1,6,2)
    bar(totNorData(2,:))
    xlabel('Avg Width / Shape')
    subplot(1,6,3)
    bar(totNorData(3,:))
    xlabel('Avg Height / Shape')
    title('Nor Data','FontWeight','bold')
    subplot(1,6,4)
    bar(totNorData(4,:))
    xlabel('Avg Curvature / Stroke')
    subplot(1,6,5)
    bar(totNorData(5,:))
    xlabel('Avg Points / Stroke')
    subplot(1,6,6)
    bar(totNorData(6,:))
    xlabel('Avg Min Dist to Gate')
    
    
    figure
    subplot(1,6,1)
    bar(totNotData(1,:))
    xlabel('Avg Pressure / Stroke')
    subplot(1,6,2)
    bar(totNotData(2,:))
    xlabel('Avg Width / Shape')
    subplot(1,6,3)
    bar(totNotData(3,:))
    xlabel('Avg Height / Shape')
    title('Not Data','FontWeight','bold')
    subplot(1,6,4)
    bar(totNotData(4,:))
    xlabel('Avg Curvature / Stroke')
    subplot(1,6,5)
    bar(totNotData(5,:))
    xlabel('Avg Points / Stroke')
    subplot(1,6,6)
    bar(totNotData(6,:))
    xlabel('Avg Min Dist to Gate')
    
    figure
    bar(percentageOfConsecutive)
    xlabel('Percentage of Shapes Drawn Consecutively')
    ylabel('%')
    title('Consecutive Data','FontWeight','bold')
    
    
    
    %totData = [avgWirePressure avgWireWidth avgWireHeight avgWireCurvature avgWireNumPoints avgWireMinDist;avgAndPressure  avgAndWidth avgAndHeight avgAndCurvature avgAndNumPoints avgAndMinDist;avgOrPressure avgOrWidth avgOrHeight avgOrCurvature avgOrNumPoints avgOrMinDist;avgXorPressure avgXorWidth avgXorHeight avgXorCurvature avgXorNumPoints avgXorMinDist;avgNandPressure avgNandWidth avgNandHeight avgNandCurvature avgNandNumPoints avgNandMinDist; avgNorPressure avgNorWidth avgNorHeight avgNorCurvature avgNorNumPoints avgNorMinDist; avgNotPressure avgNotWidth avgNotHeight avgNotCurvature avgNorNumPoints avgNotMinDist];
    
%end
    
