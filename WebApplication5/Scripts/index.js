// Index page - Search, Pagination, Filter, Delete enrollment
$(document).ready(function () {

    var storedModel = JSON.parse(
        sessionStorage.getItem("enrollmentViewModel")
    );

    var hasStoredState = false;

    if (storedModel) {

        $("#status").val(
            storedModel.status || ""
        );

        $("#studentname").val(
            storedModel.studentname || ""
        );

        $("#size").val(
            storedModel.size || 5
        );

        $("#page").val(
            storedModel.page || 1
        );

        hasStoredState = true;
    }

    FetchData();

    $('#status, #studentname, #size').change(function () {
        $('#page').val(1);
    });

    $('#searchForm').on('reset', function () {

        sessionStorage.removeItem("enrollmentViewModel");

        setTimeout(function () {

            $("#page").val(1);
            $("#status").val("");
            $("#studentname").val("");
            $("#size").val(5);

            FetchData();

        }, 0);
    });

    let totalcount = 1;
    let pagestart = 1;
    let windowsize = 5;

    function FetchData() {

        var enrollmentModel = {

            status: $("#status").val() || "",

            studentname: $("#studentname").val() || "",

            size: $("#size").val() || 5,

            page: $("#page").val() || 1
        };

        sessionStorage.setItem(
            "enrollmentViewModel",
            JSON.stringify(enrollmentModel)
        );

        $('#tableLoader').removeClass('d-none');
        $.ajax({
            url: indexActionUrl,
            type: 'POST',
            data: $("#searchForm").serialize(),
            success: function (result) {
                $('#resultContainer').html(result);
                $('#tableLoader').addClass('d-none');
                totalcount = parseInt($("#enrollcount").val()) || 0;
                buttonlist();
                
                let datashown = $("#datashown");
                datashown.empty();
                let page = parseInt($("#page").val()) || 1;
                let size = parseInt($("#size").val()) || 5;
                let start = ((page - 1) * size) + 1;
                let end = Number(start) + Number(size) - 1;
                if (end > totalcount) { end = totalcount; }
                if (totalcount === 0) {
                    datashown.html("Showing 0 to 0 of 0 entries");
                } else {
                    datashown.html(`Showing <span class="text-dark fw-bold">${start}</span> to <span class="text-dark fw-bold">${end}</span> of <span class="text-dark fw-bold">${totalcount}</span> entries`);
                }
            },
            error: function () {
                $('#tableLoader').addClass('d-none');
                alert("Error loading data");
            }
        });
    }

    $("#searchForm").submit(function (e) {
        e.preventDefault();
        pagestart = 1;
        let pagesize = parseInt($("#size").val());
        if (pagesize <= 0) $("#size").val(Number.MAX_VALUE);
        FetchData();
    });

    $("#prevbtn").on("click", function () {
        let currentpage = parseInt($("#page").val()) || 1;
        if (currentpage > 1) {
            $("#page").val(currentpage - 1);
            FetchData();
        }
    });

    $("#nextbtn").on("click", function () {
        let currentpage = parseInt($("#page").val()) || 1;
        $("#page").val(currentpage + 1);
        FetchData();
    });

    function buttonlist() {
        let buttons = $("#buttonlist");
        buttons.empty();
        let pagesize = parseInt($("#size").val()) || 5;
        let pagescount = Math.ceil(totalcount / pagesize);
        let currentpage = parseInt($("#page").val()) || 1;
        let start = Math.floor((currentpage - 1) / windowsize) * windowsize + 1;
        let end = start + windowsize - 1;
        if (end > pagescount) end = pagescount;

        for (let i = start; i <= end; i++) {
            let isActive = currentpage === i;
            buttons.append(
                `<button type="button" class="page-number-btn pageno ${isActive ? 'active' : ''}" data-page="${i}">${i}</button>`
            );
        }
        $("#prevbtn").prop("disabled", currentpage <= 1);
        $("#nextbtn").prop("disabled", currentpage >= pagescount || pagescount === 0);
    }

    $(document).on("click", ".pageno", function () {
        let page = $(this).data("page");
        $("#page").val(page);
        FetchData();
    });

    $("#size").on("change", function () {
        $("#page").val(1);
        FetchData();
    });

    $("#toggleFilter").click(function () {
        $(this).toggleClass("active");
        
        // Modern smooth jQuery slide animation instead of simple toggleClass
        $("#filterSection").slideToggle(300);

        // Rotate or swap toggle icon state if desired
        let icon = $(this).find("i");
        if (icon.hasClass("bi-funnel")) {
            icon.removeClass("bi-funnel").addClass("bi-funnel-fill");
        } else {
            icon.removeClass("bi-funnel-fill").addClass("bi-funnel");
        }
    });
    $(document).on("click", ".edit-btn", function () {

        sessionStorage.setItem(
            "enrollment_status",
            $("#status").val() || ""
        );

        sessionStorage.setItem(
            "enrollment_studentname",
            $("#studentname").val() || ""
        );

        sessionStorage.setItem(
            "enrollment_size",
            $("#size").val() || "5"
        );

        sessionStorage.setItem(
            "enrollment_page",
            $("#page").val() || "1"
        );
    });
    // Delete enrollment handler (single registration)
    $(document).on('click', '.delete-btn', function () {

        var id = $(this).data('id');

        iziToast.question({
            timeout: false,
            close: false,
            overlay: true,
            displayMode: 'once',
            id: 'delete-confirm',
            title: 'Confirm',
            message: 'Delete Enrollment ID ' + id + '?',
            position: 'center',

            maxWidth: 700,
            layout: 2,
            buttons: [

                ['<button style="background:#4f46e5;color:white;border:none;">Delete</button>', function (instance, toast) {

                    $.post(deleteEnrollmentUrl,
                        { id: id })
                        .done(function (result) {

                            instance.hide({ transitionOut: 'fadeOut' }, toast);

                            iziToast.success({
                                title: 'Success',
                                message: result.message,
                                position: 'topRight'
                            });

                            FetchData();
                        })
                        .fail(function () {

                            instance.hide({ transitionOut: 'fadeOut' }, toast);
                            iziToast.error({
                                title: 'Error',
                                message: 'Error deleting enrollment.',
                                position: 'topRight'
                            });
                        });

                }, true],

                ['<button style="background:white;color:#64748b;border:1px solid #cbd5e1;">Cancel</button>', function (instance, toast) {

                    instance.hide({ transitionOut: 'fadeOut' }, toast);

                }]
            ]
        });
    });

    $(document).on('click', '#btnAddEnrollment', function () {

        $.get('/Home/InsertEnrollment', function (html) {

            $('#enrollmentModalBody').html(html);

            var modal = bootstrap.Modal.getOrCreateInstance(
                document.getElementById('enrollmentModal')
            );

            modal.show();
        });
    });

    $(document).on('click', '.edit-btn', function () {

        var id = $(this).data('id');

        $.get('/Home/InsertEnrollment',
            { id: id },
            function (html) {

                $('#enrollmentModalBody').html(html);

                var modal = bootstrap.Modal.getOrCreateInstance(
                    document.getElementById('enrollmentModal')
                );

                modal.show();
            });
    });

    $(document).on('click', '#btnSubmit', function () {
        var enrollmentId = $("#EnrollmentID").val();
        var studentId = $("#StudentID").val();
        var courseOfferingId = $("#CourseOfferingID").val();
        var enrollmentDate = $("#EnrollmentDate").val();
        var status = $("#Status").val();

        if (!studentId || studentId === "") {
            showMessage("Please select a Student.", "danger");
            return;
        }
        if (!courseOfferingId || courseOfferingId === "") {
            showMessage("Please select a Course Offering.", "danger");
            return;
        }
        if (!enrollmentDate) {
            showMessage("Please select an Enrollment Date.", "danger");
            return;
        }
        if (!status || status === "") {
            showMessage("Please select a Status.", "danger");
            return;
        }

        var data = {
            EnrollmentID: enrollmentId ? parseInt(enrollmentId) : null,
            StudentID: parseInt(studentId),
            CourseOfferingID: parseInt(courseOfferingId),
            EnrollmentDate: enrollmentDate,
            Status: status
        };

        $.ajax({
            url: '/Home/InsertEnrollment',
            type: 'POST',
            data: data,
            success: function (result) {
                if (result && result.toLowerCase().indexOf("success") !== -1) {
                    showMessage(result, "success");
                    setTimeout(function () {
                        var modalInstance = bootstrap.Modal.getInstance(document.getElementById('enrollmentModal'));
                        if (modalInstance) {
                            modalInstance.hide();
                        }
                        FetchData();
                    }, 1500); 
                } else {
                    showMessage(result, "info");
                }
            },
            error: function () {
                showMessage("An error occurred. Please try again.", "danger");
            }
        });
    });

    function showMessage(msg, type) {
        var icon = type === "success" ? "bi-check-circle-fill"
                 : type === "danger" ? "bi-exclamation-triangle-fill"
                 : "bi-info-circle-fill";

        $("#message").html(
            '<div class="alert alert-' + type + ' d-flex align-items-center" role="alert">' +
                '<i class="bi ' + icon + ' me-2 fs-5"></i>' +
                '<div>' + msg + '</div>' +
            '</div>'
        );

        setTimeout(function () {
            $("#message").fadeOut(400, function () {
                $(this).html("").show();
            });
        }, 5000);
    }

});
