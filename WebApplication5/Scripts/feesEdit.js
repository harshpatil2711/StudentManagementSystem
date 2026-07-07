$(document).ready(function () {

    $('#studentSelect').select2({
        theme: 'bootstrap-5',
        width: '100%',
        placeholder: '-- Select Student --',
        allowClear: true
    });

    $('#studentSelect').on('change', function () {
        var studentId = $(this).val();
        if (studentId) {
            loadEnrollments(studentId);
        } else {
            $('#enrollmentTableWrapper').hide();
            $('#selectStudentPrompt').show();
        }
    });

    function loadEnrollments(studentId) {
        $.ajax({
            url: getStudentEnrollmentsUrl,
            type: 'GET',
            data: { studentId: parseInt(studentId) },
            beforeSend: function () {
                $('#enrollmentTableBody').html('<tr><td colspan="9" class="text-center py-4"><div class="spinner-border text-primary" role="status"></div><div class="mt-2 text-muted">Loading...</div></td></tr>');
                $('#enrollmentTableWrapper').show();
                $('#selectStudentPrompt').hide();
            },
            success: function (data) {
                if (data && data.length > 0) {
                    renderTable(data);
                    $('#noDataMessage').addClass('d-none');
                } else {
                    $('#enrollmentTableBody').html('');
                    $('#noDataMessage').removeClass('d-none');
                }
            },
            error: function () {
                $('#enrollmentTableBody').html('<tr><td colspan="9" class="text-center py-4 text-danger"><i class="bi bi-exclamation-triangle me-2"></i>Error loading data.</td></tr>');
            }
        });
    }

    function renderTable(data) {
        var html = '';
        $.each(data, function (i, item) {
            var totalFees = item.TotalFees != null ? 'Rs. ' + Number(item.TotalFees).toFixed(2) : '<span class="text-muted">Not set</span>';
            var feesPaid = item.FeesPaid != null ? 'Rs. ' + Number(item.FeesPaid).toFixed(2) : 'Rs. 0.00';
            var remainingFees = Number(item.RemainingFees).toFixed(2);
            var remainingClass = remainingFees > 0 ? 'text-danger' : 'text-success';
            var statusBadge = getStatusBadge(item.Status);

            html += '<tr>' +
                '<td class="fw-bold">#' + item.EnrollmentID + '</td>' +
                '<td class="fw-bold">' + escapeHtml(item.CourseName) + '</td>' +
                '<td><span class="badge ' + (item.CourseType === 'Skill' ? 'bg-info text-dark' : 'bg-secondary text-white') + '">' + escapeHtml(item.CourseType) + '</span></td>' +
                '<td>' + formatDate(item.EnrollmentDate) + '</td>' +
                '<td>' + statusBadge + '</td>' +
                '<td class="fw-bold">' + totalFees + '</td>' +
                '<td class="fw-bold">' + feesPaid + '</td>' +
                '<td class="fw-bold ' + remainingClass + '">Rs. ' + remainingFees + '</td>' +
                '<td><button class="btn btn-sm btn-premium-outline fw-bold edit-fee-btn" data-id="' + item.EnrollmentID + '" data-course="' + escapeHtml(item.CourseName) + '" data-total="' + (item.TotalFees || 0) + '" data-paid="' + (item.FeesPaid || 0) + '" data-remaining="' + remainingFees + '" title="Edit Fee"><i class="bi bi-pencil-square"></i></button></td>' +
                '</tr>';
        });
        $('#enrollmentTableBody').html(html);
    }

    function getStatusBadge(status) {
        if (!status) return '<span class="badge bg-secondary">Unknown</span>';
        var s = status.toLowerCase();
        var cls = 'badge-status-pending';
        if (s.indexOf('active') !== -1 || s.indexOf('approved') !== -1) cls = 'badge-status-active';
        else if (s.indexOf('complete') !== -1 || s.indexOf('success') !== -1) cls = 'badge-status-completed';
        else if (s.indexOf('cancel') !== -1 || s.indexOf('drop') !== -1 || s.indexOf('reject') !== -1) cls = 'badge-status-cancelled';
        return '<span class="badge-status ' + cls + '">' + escapeHtml(status) + '</span>';
    }

    function formatDate(dateStr) {
        if (!dateStr) return '';
        var d = new Date(dateStr);
        if (isNaN(d.getTime())) return dateStr;
        var year = d.getFullYear();
        var month = String(d.getMonth() + 1).padStart(2, '0');
        var day = String(d.getDate()).padStart(2, '0');
        return year + '-' + month + '-' + day;
    }

    function escapeHtml(str) {
        if (!str) return '';
        return String(str).replace(/[&<>"']/g, function (m) {
            if (m === '&') return '&amp;';
            if (m === '<') return '&lt;';
            if (m === '>') return '&gt;';
            if (m === '"') return '&quot;';
            if (m === "'") return '&#039;';
            return m;
        });
    }

    $(document).on('click', '.edit-fee-btn', function () {
        var btn = $(this);
        $('#editEnrollmentId').val(btn.data('id'));
        $('#editCourseName').text(btn.data('course'));
        var total = parseFloat(btn.data('total')) || 0;
        var paid = parseFloat(btn.data('paid')) || 0;
        var remaining = parseFloat(btn.data('remaining')) || 0;
        $('#editTotalFees').text('Rs. ' + total.toFixed(2));
        $('#editFeesPaid').val(paid.toFixed(2));
        $('#editRemainingFees').text('Rs. ' + remaining.toFixed(2));
        bootstrap.Modal.getOrCreateInstance(document.getElementById('feeEditModal')).show();
    });

    $('#btnSaveFeeEdit').on('click', function () {
        var enrollmentId = parseInt($('#editEnrollmentId').val());
        var totalFees = parseFloat($('#editTotalFees').text().replace('Rs. ', '')) || 0;
        var feesPaid = parseFloat($('#editFeesPaid').val()) || 0;

        if (feesPaid < 0) {
            iziToast.error({ title: 'Error', message: 'Fees Paid cannot be negative.', position: 'topRight' });
            return;
        }

        $.ajax({
            url: saveFeeUrl,
            type: 'POST',
            data: { enrollmentId: enrollmentId, totalFees: totalFees, feesPaid: feesPaid },
            success: function (result) {
                if (typeof result === 'string' && result.toLowerCase().indexOf('success') !== -1) {
                    iziToast.success({ title: 'Success', message: result, position: 'topRight' });
                    $('#feeEditModal').modal('hide');
                    var studentId = $('#studentSelect').val();
                    if (studentId) loadEnrollments(studentId);
                } else {
                    iziToast.error({ title: 'Error', message: result, position: 'topRight' });
                }
            },
            error: function () {
                iziToast.error({ title: 'Error', message: 'An error occurred. Please try again.', position: 'topRight' });
            }
        });
    });

    $('#studentSelect').trigger('change');
});