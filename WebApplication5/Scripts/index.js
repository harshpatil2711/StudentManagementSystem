// Index page - Search, Pagination, Filter, Delete enrollment, Sort columns
$(document).ready(function () {

    // ── Sort State ────────────────────────────────────────────────────
    var currentSortColumn    = "EnrollmentId";
    var currentSortDirection = "ASC";

    var storedModel = JSON.parse(
        sessionStorage.getItem("enrollmentViewModel")
    );

    if (storedModel) {

        $("#status").val(storedModel.status || "");
        $("#studentname").val(storedModel.studentname || "");

        if (storedModel.CourseIDs) {
            $("#courseIDs").val(storedModel.CourseIDs.split(",")).trigger('change');
        }

        $("#size").val(storedModel.size || 5);
        $("#page").val(storedModel.page || 1);

        if (storedModel.SortColumn)    currentSortColumn    = storedModel.SortColumn;
        if (storedModel.SortDirection) currentSortDirection = storedModel.SortDirection;
    }

    // ── Sort Arrow Rendering ──────────────────────────────────────────
    function updateSortArrows() {
        $("th[data-sort]").each(function () {
            $(this).find(".sort-icon").html("&#8597;").css("opacity", "0.35");
        });
        var $active = $("th[data-sort='" + currentSortColumn + "'] .sort-icon");
        $active
            .html(currentSortDirection === "ASC" ? "&#8593;" : "&#8595;")
            .css("opacity", "1");
    }

    // ── Select2 Setup ─────────────────────────────────────────────────
    function updateSelectionDisplay() {
        var select2El = document.querySelector('#courseIDs + .select2 .select2-selection--multiple');
        if (!select2El) return;
        var $rendered = $(select2El).find('.select2-selection__rendered');
        var selected = ($('#courseIDs').val() || []).filter(function (v) { return v !== '__all__'; });
        $rendered.find('.select2-selection__count-badge').remove();
        $rendered.find('.select2-selection__choice').removeAttr('style');
        if (selected.length > 2) {
            $rendered.find('.select2-selection__choice:gt(1)').hide();
            $rendered.append(
                '<span class="select2-selection__count-badge">+' + (selected.length - 2) + '</span>'
            );
        }
    }

    $('#courseIDs').select2({
        theme: 'bootstrap-5',
        width: '100%',
        placeholder: 'Search courses...',
        allowClear: true,
        closeOnSelect: false
    });

    updateSelectionDisplay();

    var updatingSelectAll = false;
    $('#courseIDs').on('select2:select', function (e) {
        if (e.params.data.id === '__all__' && !updatingSelectAll) {
            updatingSelectAll = true;
            var allValues = $('#courseIDs option').map(function () {
                return this.value !== '__all__' ? this.value : null;
            }).get();
            $('#courseIDs').val(allValues).trigger('change');
            updatingSelectAll = false;
        }
        updateSelectionDisplay();
    });
    $('#courseIDs').on('select2:unselect', function (e) {
        if (e.params.data.id === '__all__' && !updatingSelectAll) {
            updatingSelectAll = true;
            $('#courseIDs').val([]).trigger('change');
            updatingSelectAll = false;
        }
        updateSelectionDisplay();
    });

    // ── Initial Load ──────────────────────────────────────────────────
    FetchData();

    $('#status, #studentname, #size, #courseIDs').change(function () {
        $('#page').val(1);
    });

    // ── Reset Form ────────────────────────────────────────────────────
    $('#searchForm').on('reset', function () {
        sessionStorage.removeItem("enrollmentViewModel");
        setTimeout(function () {
            $("#page").val(1);
            $("#status").val("");
            $("#studentname").val("");
            $("#size").val(5);
            $("#courseIDs").val([]).trigger('change');
            currentSortColumn    = "EnrollmentId";
            currentSortDirection = "ASC";
            FetchData();
        }, 0);
    });

    // ── Sort Column Click ─────────────────────────────────────────────
    $(document).on("click", "th[data-sort]", function () {
        var col = $(this).data("sort");
        if (currentSortColumn === col) {
            currentSortDirection = currentSortDirection === "ASC" ? "DESC" : "ASC";
        } else {
            currentSortColumn    = col;
            currentSortDirection = "ASC";
        }
        $("#page").val(1);
        FetchData();
    });

    // ── Pagination State ──────────────────────────────────────────────
    let totalcount = 1;
    let windowsize = 5;

    // ── Main AJAX Fetch ───────────────────────────────────────────────
    function FetchData() {

        var enrollmentModel = {
            status:        $("#status").val() || "",
            CourseIDs:     ($("#courseIDs").val() || []).join(","),
            studentname:   $("#studentname").val() || "",
            size:          $("#size").val() || 5,
            page:          $("#page").val() || 1,
            SortColumn:    currentSortColumn,
            SortDirection: currentSortDirection
        };

        sessionStorage.setItem("enrollmentViewModel", JSON.stringify(enrollmentModel));

        $('#tableLoader').removeClass('d-none');

        $.ajax({
            url: indexActionUrl,
            type: 'POST',
            data: enrollmentModel,
            success: function (result) {
                $('#resultContainer').html(result);
                $('#tableLoader').addClass('d-none');
                totalcount = parseInt($("#enrollcount").val()) || 0;

                // Sync sort state from hidden fields echoed by the partial view
                var sc = $("#sortColumn").val();
                var sd = $("#sortDirection").val();
                if (sc) currentSortColumn    = sc;
                if (sd) currentSortDirection = sd;

                updateSortArrows();
                buttonlist();

                let datashown = $("#datashown");
                datashown.empty();
                let page  = parseInt($("#page").val()) || 1;
                let size  = parseInt($("#size").val()) || 5;
                let start = ((page - 1) * size) + 1;
                let end   = Number(start) + Number(size) - 1;
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

    // ── Search Submit ─────────────────────────────────────────────────
    $("#searchForm").submit(function (e) {
        e.preventDefault();
        let pagesize = parseInt($("#size").val());
        if (pagesize <= 0) $("#size").val(Number.MAX_VALUE);
        FetchData();
    });

    // ── Prev / Next ───────────────────────────────────────────────────
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

    // ── Page Number Buttons ───────────────────────────────────────────
    function buttonlist() {
        let buttons     = $("#buttonlist");
        buttons.empty();
        let pagesize    = parseInt($("#size").val()) || 5;
        let pagescount  = Math.ceil(totalcount / pagesize);
        let currentpage = parseInt($("#page").val()) || 1;
        let start       = Math.floor((currentpage - 1) / windowsize) * windowsize + 1;
        let end         = start + windowsize - 1;
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
        $("#page").val($(this).data("page"));
        FetchData();
    });

    $("#size").on("change", function () {
        $("#page").val(1);
        FetchData();
    });

    // ── Filter Toggle ─────────────────────────────────────────────────
    $("#toggleFilter").click(function () {
        $(this).toggleClass("active");
        $("#filterSection").slideToggle(300);
        let icon = $(this).find("i");
        if (icon.hasClass("bi-funnel")) {
            icon.removeClass("bi-funnel").addClass("bi-funnel-fill");
        } else {
            icon.removeClass("bi-funnel-fill").addClass("bi-funnel");
        }
    });

    // ── Edit Button ───────────────────────────────────────────────────
    $(document).on("click", ".edit-btn", function () {
        sessionStorage.setItem("enrollment_status",      $("#status").val() || "");
        sessionStorage.setItem("enrollment_studentname", $("#studentname").val() || "");
        sessionStorage.setItem("enrollment_size",        $("#size").val() || "5");
        sessionStorage.setItem("enrollment_page",        $("#page").val() || "1");

        var id = $(this).data('id');
        $.get('/Home/InsertEnrollment', { id: id }, function (html) {
            $('#enrollmentModalBody').html(html);
            bootstrap.Modal.getOrCreateInstance(
                document.getElementById('enrollmentModal')
            ).show();
        });
    });

    // ── Add New Enrollment ────────────────────────────────────────────
    $(document).on('click', '#btnAddEnrollment', function () {
        $.get('/Home/InsertEnrollment', function (html) {
            $('#enrollmentModalBody').html(html);
            bootstrap.Modal.getOrCreateInstance(
                document.getElementById('enrollmentModal')
            ).show();
        });
    });

    // ── Delete Enrollment ─────────────────────────────────────────────
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
                    $.post(deleteEnrollmentUrl, { id: id })
                        .done(function (result) {
                            instance.hide({ transitionOut: 'fadeOut' }, toast);
                            iziToast.success({ title: 'Success', message: result.message, position: 'topRight' });
                            FetchData();
                        })
                        .fail(function () {
                            instance.hide({ transitionOut: 'fadeOut' }, toast);
                            iziToast.error({ title: 'Error', message: 'Error deleting enrollment.', position: 'topRight' });
                        });
                }, true],
                ['<button style="background:white;color:#64748b;border:1px solid #cbd5e1;">Cancel</button>', function (instance, toast) {
                    instance.hide({ transitionOut: 'fadeOut' }, toast);
                }]
            ]
        });
    });

    // ── Save Enrollment (modal form submit) ───────────────────────────
    $(document).on('click', '#btnSubmit', function () {
        var enrollmentId     = $("#EnrollmentID").val();
        var studentId        = $("#StudentID").val();
        var courseOfferingId = $("#CourseOfferingID").val();
        var enrollmentDate   = $("#EnrollmentDate").val();
        var status           = $("#Status").val();

        if (!studentId || studentId === "") { showMessage("Please select a Student.", "danger"); return; }
        if (!courseOfferingId || courseOfferingId === "") { showMessage("Please select a Course Offering.", "danger"); return; }
        if (!enrollmentDate) { showMessage("Please select an Enrollment Date.", "danger"); return; }
        if (!status || status === "") { showMessage("Please select a Status.", "danger"); return; }

        $.ajax({
            url: '/Home/InsertEnrollment',
            type: 'POST',
            data: {
                EnrollmentID:     enrollmentId ? parseInt(enrollmentId) : null,
                StudentID:        parseInt(studentId),
                CourseOfferingID: parseInt(courseOfferingId),
                EnrollmentDate:   enrollmentDate,
                Status:           status
            },
            success: function (result) {
                if (result && result.toLowerCase().indexOf("success") !== -1) {
                    iziToast.success({ title: 'Success', message: result, position: 'topRight' });
                    $('#enrollmentModal').modal('hide');
                    FetchData();
                } else {
                    showMessage(result, "danger");
                }
            },
            error: function () {
                showMessage("An error occurred. Please try again.", "danger");
            }
        });
    });

    // ── Show Alert Message ────────────────────────────────────────────
    function showMessage(msg, type) {
        var icon = type === "success" ? "bi-check-circle-fill"
            : type === "danger"  ? "bi-exclamation-triangle-fill"
            : "bi-info-circle-fill";

        $("#message").html(
            '<div class="alert alert-' + type + ' d-flex align-items-center" role="alert">' +
            '<i class="bi ' + icon + ' me-2 fs-5"></i>' +
            '<div>' + msg + '</div>' +
            '</div>'
        );

        setTimeout(function () {
            $("#message").fadeOut(400, function () { $(this).html("").show(); });
        }, 5000);
    }
});
