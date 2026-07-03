// InsertEnrollment page — Skill modal with months, table display, form submit
$(document).ready(function () {
    var selectedSkills = []; // { SkillID, SkillName, SkillFees, Months }

    // Load skills when course offering changes
    $(document).on('change', '#CourseOfferingID', function () {
        var offeringId = $(this).val();
        if (offeringId) {
            loadCourseType(offeringId);
        } else {
            $('#skillsSection, #academicFeeSection').hide();
            $('#skillsTableSection').hide();
        }
    });

    function loadCourseType(courseOfferingId) {
        $.get('/Home/GetCourseType', { courseOfferingId: courseOfferingId }, function (result) {
            if (result.courseType === 'Skill') {
                $('#academicFeeSection').hide();
                $('#skillsSection').show();
                // If edit mode, preload skills
                var enrollId = $('#EnrollmentID').val();
                if (enrollId && parseInt(enrollId) > 0) {
                    loadSkillsFromServer(courseOfferingId, true);
                } else {
                    loadSkillsFromServer(courseOfferingId, false);
                }
            } else {
                $('#skillsSection, #skillsTableSection').hide();
                selectedSkills = [];
                updateHiddenField();
                $.get('/Home/GetCourseOfferingFee', { courseOfferingId: courseOfferingId }, function (r) {
                    $('#academicFeeDisplay').text(parseFloat(r.fee).toFixed(2));
                    $('#academicFeeSection').show();
                });
            }
        });
    }

    function loadSkillsFromServer(courseOfferingId, isEdit) {
        $.get(getSkillsByOfferingUrl, { courseOfferingId: courseOfferingId }, function (skills) {
            // If edit mode, restore selectedSkills from initialSelectedSkills
            if (isEdit && initialSelectedSkills) {
                selectedSkills = [];
                var pairs = initialSelectedSkills.split(',');
                $.each(pairs, function (i, pair) {
                    var parts = pair.split(':');
                    if (parts.length === 2) {
                        var sid = parseInt(parts[0]);
                        var m = parseInt(parts[1]) || 1;
                        var skill = null;
                        $.each(skills, function (j, s) {
                            if (s.SkillID === sid) { skill = s; return false; }
                        });
                        if (skill) {
                            selectedSkills.push({
                                SkillID: skill.SkillID,
                                SkillName: skill.SkillName,
                                SkillFees: skill.SkillFees,
                                Months: m
                            });
                        }
                    }
                });
                updateSkillsTable();
                updateHiddenField();
            }
        });
    }

    // Open modal
    $(document).on('click', '#btnAddSkills', function () {
        var offeringId = $('#CourseOfferingID').val();
        if (!offeringId) { return; }

        $.get(getSkillsByOfferingUrl, { courseOfferingId: offeringId }, function (skills) {
            if (!skills || skills.length === 0) {
                iziToast.warning({ title: 'No Skills', message: 'No skills available for this course.', position: 'topCenter', timeout: 3000 });
                return;
            }
            var html = '';
            $.each(skills, function (i, s) {
                var checked = '';
                var months = 1;
                // Check if already selected
                $.each(selectedSkills, function (j, sel) {
                    if (sel.SkillID === s.SkillID) {
                        checked = 'checked';
                        months = sel.Months;
                        return false;
                    }
                });
                html += '<div class="skill-modal-item border rounded p-2 mb-2 bg-white">' +
                    '<div class="d-flex align-items-center">' +
                    '<input class="form-check-input skill-modal-checkbox me-2" type="checkbox" value="' + s.SkillID + '" ' + checked +
                    ' data-fees="' + s.SkillFees + '" data-name="' + s.SkillName + '">' +
                    '<div class="flex-grow-1">' +
                    '<strong>' + s.SkillName + '</strong>' +
                    ' <span class="text-muted small">(Rs. ' + parseFloat(s.SkillFees).toFixed(2) + '/month)</span>' +
                    '</div>' +
                    '<div class="d-flex align-items-center ms-2">' +
                    '<label class="small me-1 fw-semibold">Months:</label>' +
                    '<input type="number" class="form-control form-control-sm skill-modal-months" style="width:60px;" value="' + months + '" min="1" data-skillid="' + s.SkillID + '">' +
                    '</div>' +
                    '</div>' +
                    '<div class="mt-1 text-end small fw-bold skill-modal-row-total" data-skillid="' + s.SkillID + '">' +
                    'Fee: Rs. ' + (parseFloat(s.SkillFees) * months).toFixed(2) +
                    '</div>' +
                    '</div>';
            });
            $('#skillListContainer').html(html);
            updateModalTotal();
            var modal = new bootstrap.Modal(document.getElementById('skillModal'));
            modal.show();
        });
    });

    // Recalculate row total when months changes
    $(document).on('input', '.skill-modal-months', function () {
        var skillId = $(this).data('skillid');
        var months = parseInt($(this).val()) || 1;
        if (months < 1) { $(this).val(1); months = 1; }
        var checkbox = $(this).closest('.skill-modal-item').find('.skill-modal-checkbox');
        var fees = parseFloat(checkbox.data('fees')) || 0;
        $('.skill-modal-row-total[data-skillid="' + skillId + '"]').text('Fee: Rs. ' + (fees * months).toFixed(2));
        updateModalTotal();
    });

    // Recalculate on checkbox change
    $(document).on('change', '.skill-modal-checkbox', function () {
        var $item = $(this).closest('.skill-modal-item');
        var skillId = $(this).val();
        var months = parseInt($item.find('.skill-modal-months').val()) || 1;
        var fees = parseFloat($(this).data('fees')) || 0;
        if ($(this).is(':checked')) {
            $('.skill-modal-row-total[data-skillid="' + skillId + '"]').text('Fee: Rs. ' + (fees * months).toFixed(2));
        } else {
            $('.skill-modal-row-total[data-skillid="' + skillId + '"]').text('Fee: Rs. 0.00');
        }
        updateModalTotal();
    });

    function updateModalTotal() {
        var total = 0;
        $('.skill-modal-checkbox:checked').each(function () {
            var $item = $(this).closest('.skill-modal-item');
            var months = parseInt($item.find('.skill-modal-months').val()) || 1;
            total += parseFloat($(this).data('fees')) * months;
        });
        $('#skillModalTotal').text(total.toFixed(2));
    }

    // Confirm modal selection
    $(document).on('click', '#btnAddSkillsConfirm', function () {
        selectedSkills = [];
        $('.skill-modal-checkbox:checked').each(function () {
            var $item = $(this).closest('.skill-modal-item');
            var skillId = parseInt($(this).val());
            var skillName = $(this).data('name');
            var fees = parseFloat($(this).data('fees'));
            var months = parseInt($item.find('.skill-modal-months').val()) || 1;
            selectedSkills.push({
                SkillID: skillId,
                SkillName: skillName,
                SkillFees: fees,
                Months: months
            });
        });
        updateSkillsTable();
        updateHiddenField();
        bootstrap.Modal.getInstance(document.getElementById('skillModal')).hide();
    });

    // Remove skill from table
    $(document).on('click', '.remove-skill-btn', function () {
        var skillId = parseInt($(this).data('skillid'));
        selectedSkills = $.grep(selectedSkills, function (s) { return s.SkillID !== skillId; });
        updateSkillsTable();
        updateHiddenField();
    });

    function updateSkillsTable() {
        var $tbody = $('#skillsTableBody');
        $tbody.empty();
        if (selectedSkills.length === 0) {
            $('#skillsTableSection').hide();
            return;
        }
        $('#skillsTableSection').show();
        var grandTotal = 0;
        $.each(selectedSkills, function (i, s) {
            var total = s.SkillFees * s.Months;
            grandTotal += total;
            $tbody.append(
                '<tr>' +
                '<td class="fw-bold">' + s.SkillName + '</td>' +
                '<td>Rs. ' + parseFloat(s.SkillFees).toFixed(2) + '</td>' +
                '<td>' + s.Months + '</td>' +
                '<td class="fw-bold">Rs. ' + total.toFixed(2) + '</td>' +
                '<td class="text-center"><button type="button" class="btn btn-sm btn-outline-danger remove-skill-btn" data-skillid="' + s.SkillID + '" title="Remove"><i class="bi bi-x-lg"></i></button></td>' +
                '</tr>'
            );
        });
        $('#grandTotalDisplay').text('Rs. ' + grandTotal.toFixed(2));
    }

    function updateHiddenField() {
        var data = [];
        $.each(selectedSkills, function (i, s) {
            data.push(s.SkillID + ':' + s.Months);
        });
        $('#SelectedSkills').val(data.join(','));
    }

    // Form submit
    $("#btnSubmit").click(function () {
        var enrollmentId     = $("#EnrollmentID").val();
        var studentId        = $("#StudentID").val();
        var courseOfferingId = $("#CourseOfferingID").val();
        var enrollmentDate   = $("#EnrollmentDate").val();
        var status           = $("#Status").val();

        if (!studentId || studentId === "") { showMessage("Please select a Student.", "danger"); return; }
        if (!courseOfferingId || courseOfferingId === "") { showMessage("Please select a Course Offering.", "danger"); return; }
        if (!enrollmentDate) { showMessage("Please select an Enrollment Date.", "danger"); return; }
        if (!status || status === "") { showMessage("Please select a Status.", "danger"); return; }

        if ($('#skillsSection').is(':visible') && !$('#SelectedSkills').val()) {
            showMessage("Please select at least one skill.", "danger"); return;
        }

        $.ajax({
            url: insertEnrollmentUrl,
            type: 'POST',
            data: {
                EnrollmentID:     enrollmentId ? parseInt(enrollmentId) : null,
                StudentID:        parseInt(studentId),
                CourseOfferingID: parseInt(courseOfferingId),
                EnrollmentDate:   enrollmentDate,
                Status:           status,
                SelectedSkills:   $('#SelectedSkills').val() || ''
            },
            success: function (result) {
                if (typeof result === 'string' && result.toLowerCase().indexOf("success") !== -1) {
                    iziToast.success({ title: 'Success', message: result, position: 'topRight' });
                    setTimeout(function () {
                        window.location.href = indexUrl;
                    }, 1500);
                } else {
                    showMessage(result, "danger");
                }
            },
            error: function () {
                showMessage("An error occurred. Please try again.", "danger");
            }
        });
    });

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

    // Trigger course type load if already selected (e.g., edit mode)
    var initialOfferingId = $('#CourseOfferingID').val();
    if (initialOfferingId) {
        loadCourseType(initialOfferingId);
    }
});
