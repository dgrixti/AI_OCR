; (function () {0

    var clearBtn = document.getElementById("resetButton");

    const canvas = document.getElementById('drawCanvas');
    const ctx = canvas.getContext('2d');

    var strokesArr = new Array();
    var isDrawing = false;

    clearBtn.addEventListener("click", function (e) {

        clearMouseStrokes();

    }, false);

    function clearMouseStrokes() {

        // clear strokes
        strokesArr = new Array();
        isDrawing = false;

        // clear main
        ctx.clearRect(0, 0, ctx.canvas.width, ctx.canvas.height);

        // clear preview
        var canvasThumb = document.getElementById('drawCanvasThumb');
        const ctxThumb = canvasThumb.getContext('2d');
        ctxThumb.clearRect(0, 0, ctxThumb.canvas.width, ctxThumb.canvas.height);

        document.getElementById("charValueOutput").value = "";

        if (document.getElementById("charValueOutput2") != null && document.getElementById("charValueOutput2") != 'undefined') {
            document.getElementById("charValueOutput2").value = "";
        }
    }

    function addMouseStroke(mouseX, mouseY, isDrag) {
        var strokeObj = {};
        strokeObj.mouseX = mouseX;
        strokeObj.mouseY = mouseY;
        strokeObj.isDrag = isDrag;
        strokesArr.push(strokeObj);
    }

    function renderCanvas() {
        ctx.clearRect(0, 0, ctx.canvas.width, ctx.canvas.height);

        ctx.strokeStyle = "black";
        ctx.lineJoin = "round";
        ctx.lineWidth = 10;

        // Recreate the line drawing on canvas from all the collective recorded points.
        for (var i = 0; i < strokesArr.length; i++) {

            var stroke = strokesArr[i];

            ctx.beginPath();

            // If type of stroke is dragging type, move point to the previous,
            // Else start the draw line from the current stroke.
            // NB: Assuming fist stroke will never be a drag type (on mouse down) so -1 wont break it.
            if (stroke.isDrag && i) {
                var strokePrev = strokesArr[i - 1];
                ctx.moveTo(strokePrev.mouseX, strokePrev.mouseY);
            } else {
                ctx.moveTo(stroke.mouseX - 1, stroke.mouseY);
            }

            // Draw the line
            ctx.lineTo(stroke.mouseX, stroke.mouseY);

            ctx.closePath();
            ctx.stroke();
        }
    }

    // Desktop (mouse)

    canvas.addEventListener("mousedown", function (e) {

        if (e.target == canvas) {
            e.preventDefault();
        }
        var rect = canvas.getBoundingClientRect();
        var mouseX = e.clientX - rect.left;;
        var mouseY = e.clientY - rect.top;
        isDrawing = true;
        addMouseStroke(mouseX, mouseY, false);
        renderCanvas();


    }, false);

    canvas.addEventListener("mousemove", function (e) {

        if (isDrawing) {
            var rect = canvas.getBoundingClientRect();
            var mouseX = e.clientX - rect.left;;
            var mouseY = e.clientY - rect.top;
            addMouseStroke(mouseX, mouseY, true);
            renderCanvas();
        }

    }, false);

    canvas.addEventListener("mouseup", function (e) {

        isDrawing = false;

        getMnistDataV2();

    }, false);


    // Mobile (touch)

    canvas.addEventListener("touchstart", function (e) {

        console.log("touch screen");

        if (e.target == canvas) {
            e.preventDefault();
        }

        var rect = canvas.getBoundingClientRect();
        var touch = e.touches[0];

        var mouseX = touch.clientX - rect.left;
        var mouseY = touch.clientY - rect.top;

        isDrawing = true;
        addMouseStroke(mouseX, mouseY);
        renderCanvas();

    }, false)

    canvas.addEventListener("touchmove", function (e) {
        if (e.target == canvas) {
            e.preventDefault();
        }
        if (isDrawing) {
            var rect = canvas.getBoundingClientRect();
            var touch = e.touches[0];

            var mouseX = touch.clientX - rect.left;
            var mouseY = touch.clientY - rect.top;

            addMouseStroke(mouseX, mouseY, true);
            renderCanvas();
        }
    }, false);

    canvas.addEventListener("touchend", function (e) {
        if (e.target == canvas) {
            e.preventDefault();
        }
        isDrawing = false;

        getMnistDataV2();

    }, false);

    canvas.addEventListener("touchleave", function (e) {
        if (e.target == canvas) {
            e.preventDefault();
        }
        isDrawing = false;
    }, false);

})()