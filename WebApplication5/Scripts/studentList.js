// StudentList page - Student directory with search/filter
$(document).ready(function () {
    var allStudents = [];

    // Populate year filter
    var currentYear = new Date().getFullYear();
    for (var y = currentYear; y >= 2000; y--) {
        $("#filterYear").append('<option value="' + y + '">' + y + '</option>');
    }

    // Load students
    loadStudents();

    function loadStudents() {
        $("#tableLoader").removeClass("d-none");
        $("#studentTableContainer").addClass("d-none");
        $("#emptyState").addClass("d-none");

        $.ajax({
            url: getStudentsUrl,
            type: "GET",
            success: function (data) {
                allStudents = data;
                renderTable(allStudents);
                $("#tableLoader").addClass("d-none");
            },
            error: function () {
                $("#tableLoader").addClass("d-none");
                $("#emptyState").removeClass("d-none");
            }
        });
    }

    function renderTable(students) {
        var tbody = $("#studentTableBody");
        tbody.empty();

        if (!students || students.length === 0) {
            $("#studentTableContainer").addClass("d-none");
            $("#emptyState").removeClass("d-none");
            return;
        }

        $("#emptyState").addClass("d-none");
        $("#studentTableContainer").removeClass("d-none");

        for (var i = 0; i < students.length; i++) {
            var s = students[i];
            var initials = getInitials(s.StudentName);
            var photoHtml;
            if (s.PhotoPath) {
                photoHtml = '<img src="' + s.PhotoPath + '" class="student-thumb" alt="' + escHtml(s.StudentName) + '" />';
            } else {
                photoHtml = '<div class="student-thumb-default">' + initials + '</div>';
            }

            var row = '<tr>' +
                '<td>' + photoHtml + '</td>' +
                '<td class="fw-semibold">' + s.StudentID + '</td>' +
                '<td class="fw-semibold">' + escHtml(s.StudentName) + '</td>' +
                '<td>' + escHtml(s.Email || '') + '</td>' +
                '<td>' + escHtml(s.Phone || '') + '</td>' +
                '<td>' + escHtml(s.Gender || '') + '</td>' +
                '<td>' + (s.AdmissionYear || '') + '</td>' +
                '<td>' +
                    '<a href="' + studentDetailsUrl + '/' + s.StudentID + '" class="btn btn-sm btn-outline-primary me-1" title="View"><i class="bi bi-eye"></i></a>' +
                    '<a href="' + studentEditUrl + '/' + s.StudentID + '" class="btn btn-sm btn-outline-secondary me-1" title="Edit"><i class="bi bi-pencil"></i></a>' +
                    '<a href="' + studentDeleteUrl + '/' + s.StudentID + '" class="btn btn-sm btn-outline-danger" title="Delete"><i class="bi bi-trash"></i></a>' +
                '</td>' +
                '</tr>';
            tbody.append(row);
        }
    }

    // Search and filter
    $("#btnSearch").click(applyFilter);
    $("#btnReset").click(function () {
        $("#searchName").val("");
        $("#filterYear").val("");
        renderTable(allStudents);
    });

    $("#searchName").keyup(function (e) {
        if (e.keyCode === 13) applyFilter();
    });

    function applyFilter() {
        var name = $("#searchName").val().trim().toLowerCase();
        var year = $("#filterYear").val();

        var filtered = allStudents.filter(function (s) {
            var matchName = !name || (s.StudentName && s.StudentName.toLowerCase().indexOf(name) !== -1);
            var matchYear = !year || (s.AdmissionYear && s.AdmissionYear.toString() === year);
            return matchName && matchYear;
        });

        renderTable(filtered);
    }

    function getInitials(name) {
        if (!name) return "?";
        var parts = name.trim().split(/\s+/);
        if (parts.length >= 2) {
            return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
        }
        return parts[0].substring(0, 2).toUpperCase();
    }

    function escHtml(str) {
        if (!str) return "";
        return str.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;");
    }
});
