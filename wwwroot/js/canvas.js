; (function () {

    const canvas = document.getElementById('drawCanvas');
    const ctx = canvas.getContext('2d');

    var strokesArr = new Array();
    var isDrawing = false;

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


})()