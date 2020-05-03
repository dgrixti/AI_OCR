
//const canvas = document.getElementById('drawCanvas');
// const ctx = canvas.getContext('2d');

function centraliseAndCropCanvas(arrPixels, imgData, context, canvas) {

    // get context boun rectangle
    var boundingRectangle = getContextBoundingBox(arrPixels);//, 0.01);	  

    var centerOfMass = getImageCenterOfMass(arrPixels);
        
    var tempCanvas = document.createElement("canvas");
    tempCanvas.width = imgData.width;
    tempCanvas.height = imgData.height;
    var tempCtx = tempCanvas.getContext("2d");

    var brW = boundingRectangle.maxX + 1 - boundingRectangle.minX;
    var brH = boundingRectangle.maxY + 1 - boundingRectangle.minY;
    var scaling = 100 / (brW > brH ? brW : brH); // rescale to fit the original 100px

    // scale the image
    tempCtx.translate(canvas.width / 2, canvas.height / 2);
    tempCtx.scale(scaling, scaling);
    tempCtx.translate(-canvas.width / 2, -canvas.height / 2);

    // translate to center of mass
    tempCtx.translate(centerOfMass.transX, centerOfMass.transY);
    tempCtx.drawImage(context.canvas, 0, 0);

    return tempCanvas.toDataURL();
}

function getMnistDataV2() {

    var canvas = document.getElementById('drawCanvas');
    var context = canvas.getContext('2d');
    // Get the image from canvas
    var imgData = context.getImageData(0, 0, 100, 100);
    // Convert it to 1s and 0s array
    var arrPixels = convertToGrascaleMatrix(imgData);
    // Crop the data
    var croppedCanvasData = centraliseAndCropCanvas(arrPixels, imgData, context, canvas);

    // redraw in the 32x32 preview canvas.
    var canvasThumb = document.getElementById('drawCanvasThumb');
    var ctx1 = canvasThumb.getContext('2d');

    // clean existing from preview
    ctx1.clearRect(0, 0, ctx1.canvas.width, ctx1.canvas.height);

    //draw the base64 string into a 28x28 canvas (for resizing)
    var img = new Image();
    img.src = croppedCanvasData;
    img.addEventListener("load", function () {

        // Draw the cropped image on preview/
        ctx1.drawImage(img, 0, 0, 32, 32);

        var imgDataThumb = ctx1.getImageData(0, 0, 32, 32);

        console.log(imgDataThumb);

        // Convert the 32x32 image to 1s and 0s array
        var arrPixels = convertToGrascaleMatrix(imgDataThumb);

        // Convert to 64 bit integer array
        var matrix4x4 = count4X4MatrixSections(arrPixels);

        document.getElementById("charValue").value = matrix4x4;
    });

}

/*
* TODO: Include reference
*/
function getImageCenterOfMass(imgData) {
    var meanX = 0;
    var meanY = 0;
    var rows = imgData.length;
    var columns = imgData[0].length;
    var sumPixels = 0;
    for (var y = 0; y < rows; y++) {
        for (var x = 0; x < columns; x++) {
            var pixel = (1 - imgData[y][x]);
            sumPixels += pixel;
            meanY += y * pixel;
            meanX += x * pixel;
        }
    }
    meanX /= sumPixels;
    meanY /= sumPixels;

    var dY = Math.round(rows / 2 - meanY);
    var dX = Math.round(columns / 2 - meanX);
    return { transX: dX, transY: dY };
}

/*
    Takes the image data and converts all pixels bits into 
    a matrix with width and height based on the height and width,
    1s and 0s depending if pixel is filled or empty.
*/
function convertToGrascaleMatrix(imgData) {

    console.log(imgData.data);

    var arrPixelsBinary = [];
    var arrReturnMatrix = [];

    var counter = 0;
    // firs step, convert all the bits into pixels of 1 (painted) and 0 (blank)
    // skip 4 pixels each time 1 set of 4 bits (r, g, b, a) = 1 pixel.
    for (var i = 4; i < imgData.data.length + 4; i += 4) {

        var value = imgData.data[i - 1];
        // if not 255, it means filled somehow (not white)
        if (value == 0) {
            arrPixelsBinary[counter] = 1; // empty
        } else {  // else we put 0
            arrPixelsBinary[counter] = 0; // filled
        }

        counter++;
    }
    // reset counter for next step
    counter = 0;

    // loop height and width to return the binary 
    // array of pixels into a 2 dimentional matrix (grid)
    for (var i = 0; i < imgData.height; i++) {

        arrReturnMatrix[i] = [];

        for (var j = 0; j < imgData.width; j++) {
            arrReturnMatrix[i][j] = arrPixelsBinary[counter];
            counter++;
        }
    }

    console.log(arrReturnMatrix);

    return arrReturnMatrix;
}


/* Do a brute force going layer by layer (enlarging rectangle from center) until no filled pixels are encountered 
    and thus find the max outer bound rectangle which has empty space + treshold. 

    Adapted from: http://phrogz.net/tmp/canvas_bounding_box.html
    https://stackoverflow.com/questions/9852159/calculate-bounding-box-of-arbitrary-pixel-based-drawing

*/
function getContextBoundingBox(imgData, alphaThreshold) {

    // set default threshold (low > 0)
    if (alphaThreshold === undefined) {
        alphaThreshold = 0.1;
    }

    var rows = imgData.length;
    var columns = imgData[0].length;

    var minX = columns;
    var minY = rows;
    var maxX = -1;
    var maxY = -1;

    for (var y = 0; y < rows; y++) {
        for (var x = 0; x < columns; x++) {
            if (imgData[y][x] < alphaThreshold) {
                if (minX > x) minX = x;
                if (maxX < x) maxX = x;
                if (minY > y) minY = y;
                if (maxY < y) maxY = y;
            }
        }
    }
    return { minY: minY, minX: minX, maxY: maxY, maxX: maxX };
}

/*
 * Count the 1s and 0s inde 4x4 bound boxes and list the number
 * to produce a 64 bit integer array.
 * 
 * There is some bug about out bound exception which is thrown and hence the try,catch 
 * however the output data is intact.
 */
function count4X4MatrixSections(pixels) {

    //console.log(pixels.length);
    var wholeSequence = "";

    // count pixels sections going 4x4 each time
    var colCountMin = 0;
    var rowCountMin = 0;
    var colCountMax = 0;
    var rowCountMax = 0;

    var stepSize = 4;
    var maxStepSize = 32;

    var counting = true;
    // 0x0, 0x1, 0x2, 0x3, 
    // 1x0, 1x1, 1x2, 1x3, 
    // 2x0, 2x1, 2x2, 2x3, 
    // 3x0, 3x1, 3x2, 3x3, 

    // 0x4, 0x5, 0x6, 0x7, 
    // 1x4, 1x5, 1x6, 1x7, 
    // etc..

    rowCountMax += stepSize;

    var boxRight = 0;
    var boxDown = 0;

    totalPix2 = 0;

    var countLoop = 0;

    while (counting) {

        boxRight += 1;

        // reset here after finish?
        if (colCountMax == maxStepSize) {

            rowCountMin += stepSize;
            rowCountMax += stepSize;

            colCountMin = 0;
            colCountMax = 0; //stepSize;

            boxRight = 0;
            boxDown += 1;
        }

        colCountMax += stepSize;
        var totalPixelsFound = 0;

        try {

            for (var i = rowCountMin; i < rowCountMax; i++) {
                for (var j = colCountMin; j < colCountMax; j++) {
                    var pix = pixels[i][j];

                    if (pix == 0) {

                        //var xxx = true;
                        //var ttt = boxRight;
                        //var fff = boxDown;

                        totalPixelsFound++;
                    }
                }
            }

            wholeSequence += totalPixelsFound + ",";

            //console.log(countLoop + ": row min: " + rowCountMin + " row max: " + rowCountMax + " col min: " + colCountMin + " col max: " + colCountMax + " => " + totalPixelsFound + ",");

            countLoop++;

            colCountMin += stepSize;

            if (rowCountMin == maxStepSize) {
                console.log("BREAK LOOP");
                counting = false;
            }

        }
        catch (err) {
            counting = false; // stop the loop on exception
            console.log("EXCEPTION!");
            console.log(err.message);
        }
    }

    console.log("64 bit integer array");
    console.log(wholeSequence);

    return wholeSequence;
}