// StudentEdit page - Edit form with photo upload
$(document).ready(function () {
    var selectedFile = null;

    // Populate admission year dropdown
    var startYear = 2000;
    var currentYear = new Date().getFullYear();
    for (var year = currentYear; year >= startYear; year--) {
        var opt = '<option value="' + year + '">' + year + '</option>';
        if (initialAdmissionYear && year.toString() === initialAdmissionYear.toString()) {
            opt = '<option value="' + year + '" selected>' + year + '</option>';
        }
        $("#AdmissionYear").append(opt);
    }

    // If photo exists, show remove button
    if ($("#photoPreview").hasClass("d-none") === false && $("#photoPreview").attr("src")) {
        $("#btnRemovePhoto").removeClass("d-none");
    }

    // Photo upload zone - file input overlays the zone, no JS click needed

    $("#photoDropZone").on("dragover", function (e) {
        e.preventDefault();
        $(this).addClass("photo-upload-zone-hover");
    });

    $("#photoDropZone").on("dragleave drop", function (e) {
        e.preventDefault();
        $(this).removeClass("photo-upload-zone-hover");
    });

    $("#photoDropZone").on("drop", function (e) {
        e.preventDefault();
        var files = e.originalEvent.dataTransfer.files;
        if (files.length > 0) {
            handlePhotoSelect(files[0]);
        }
    });

    $("#photoInput").change(function () {
        if (this.files && this.files[0]) {
            handlePhotoSelect(this.files[0]);
        }
    });

    $("#btnRemovePhoto").click(function (e) {
        e.stopPropagation();
        clearPhoto();
    });

    function handlePhotoSelect(file) {
        var allowedExts = [".jpg", ".jpeg", ".png", ".webp"];
        var ext = file.name.substring(file.name.lastIndexOf(".")).toLowerCase();

        if (allowedExts.indexOf(ext) === -1) {
            showMsg("Invalid file type. Allowed: JPG, PNG, WebP.", "danger");
            return;
        }
        if (file.size > 2 * 1024 * 1024) {
            showMsg("File size exceeds 2 MB limit.", "danger");
            return;
        }

        selectedFile = file;

        var reader = new FileReader();
        reader.onload = function (e) {
            $("#photoPreview").attr("src", e.target.result).removeClass("d-none");
            $(".photo-placeholder").addClass("d-none");
            $("#btnRemovePhoto").removeClass("d-none");
        };
        reader.readAsDataURL(file);
    }

    function clearPhoto() {
        selectedFile = null;
        $("#photoInput").val("");
        $("#photoPreview").attr("src", "").addClass("d-none");
        $(".photo-placeholder").removeClass("d-none");
        $("#btnRemovePhoto").addClass("d-none");
    }

    // Update button
    $("#btnUpdate").click(function () {
        var studentName = $("#StudentName").val().trim();
        if (!studentName) {
            showMsg("Please enter a student name.", "danger");
            return;
        }

        $("#btnUpdate").prop("disabled", true).html('<span class="spinner-border spinner-border-sm me-1.5" role="status"></span>Updating...');

        var formData = new FormData();
        formData.append("StudentID", $("#StudentID").val());
        formData.append("StudentName", studentName);
        formData.append("DateOfBirth", $("#DateOfBirth").val());
        formData.append("Email", $("#Email").val());
        formData.append("Phone", $("#Phone").val());
        formData.append("Gender", $("#Gender").val());
        formData.append("AdmissionYear", $("#AdmissionYear").val());

        if (selectedFile) {
            formData.append("photo", selectedFile);
        }

        $.ajax({
            url: editStudentUrl,
            type: "POST",
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                $("#btnUpdate").prop("disabled", false).html('<i class="bi bi-check-lg me-1.5"></i>Update Student');
                if (response.toLowerCase().indexOf("success") !== -1) {
                    showMsg(response, "success");
                    setTimeout(function () { window.location.href = studentListUrl; }, 1200);
                } else {
                    showMsg(response, "danger");
                }
            },
            error: function () {
                $("#btnUpdate").prop("disabled", false).html('<i class="bi bi-check-lg me-1.5"></i>Update Student');
                showMsg("An error occurred. Please try again.", "danger");
            }
        });
    });

    function showMsg(msg, type) {
        var icon = type === "success" ? "check-circle-fill" : "exclamation-triangle-fill";
        $("#message").html(
            '<div class="alert alert-' + type + '"><i class="bi bi-' + icon + ' me-2"></i>' + msg + '</div>'
        );
    }
});
