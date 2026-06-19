# Error Handling Implementation Plan

## Changes required across 5 files

---

### 1. `Scripts/index.js`

**A. Wrap sessionStorage + JSON.parse in try-catch (lines 8-12)**

Current:
```js
var storedModel = JSON.parse(
    sessionStorage.getItem("enrollmentViewModel")
);
if (storedModel) {
```

Replace with:
```js
var storedModel = null;
try {
    var raw = sessionStorage.getItem("enrollmentViewModel");
    if (raw) storedModel = JSON.parse(raw);
} catch (e) {
    storedModel = null;
}
if (storedModel) {
```

**B. Guard `result` type check in save success handler (line 351)**

Current:
```js
if (result && result.toLowerCase().indexOf("success") !== -1) {
```

Replace with:
```js
if (typeof result === 'string' && result.toLowerCase().indexOf("success") !== -1) {
```

**C. Guard parseInt calls (line 168)**

Current:
```js
totalcount = parseInt($("#enrollcount").val()) || 0;
```

Replace with:
```js
var ec = $("#enrollcount").val();
totalcount = ec ? parseInt(ec) || 0 : 0;
```

---

### 2. `BusinessLayer/DAL/EnrollmentDAL.cs`

**A. Add using statement at top:**
```csharp
using System.Data.SqlClient;
```

**B. `GetList()` (line 20) — wrap entire method body:**

Current — no try-catch, exceptions bubble up raw.

Replace with:
```csharp
public List<Enrollment> GetList(EnrollmentViewModel enroll)
{
    List<Enrollment> list = new List<Enrollment>();
    try
    {
        DbCommand cmd = db.GetStoredProcCommand("EnrollmentDetails");
        // ... existing code ...
        return list;
    }
    catch (SqlException ex)
    {
        // Log exception
        enroll.Enrollmentcount = 0;
        return list;
    }
    catch (Exception ex)
    {
        enroll.Enrollmentcount = 0;
        return list;
    }
}
```

**C. All other methods — same pattern:**

| Method | Return type | Catch behavior |
|--------|-------------|----------------|
| `getStatusList()` | `Dictionary<int, string>` | Return empty dict |
| `getCoursesList()` | `Dictionary<int, string>` | Return empty dict |
| `GetStudents()` | `Dictionary<int, string>` | Return empty dict |
| `GetCourseOfferings()` | `Dictionary<int, string>` | Return empty dict |
| `GetEnrollmentById(int id)` | `EnrollmentInsertViewModel` | Return null |
| `DeleteEnrollmentById(int id)` | `string` | Return "Error: " + ex.Message |
| `SaveEnrollment(EnrollmentInsertViewModel vm)` | `string` | Return "Error: " + ex.Message |

**D. Add null guard for Status conversion (line 203):**

Current:
```csharp
db.AddInParameter(cmd, "@Status", DbType.Int32, Convert.ToInt32(vm.Status));
```

Replace with:
```csharp
db.AddInParameter(cmd, "@Status", DbType.Int32,
    !string.IsNullOrEmpty(vm.Status) ? Convert.ToInt32(vm.Status) : DBNull.Value);
```

---

### 3. `BusinessLayer/DAL/StudentDAL.cs`

**A. Add using statement at top:**
```csharp
using System.Data.SqlClient;
```

**B. `InsertStudent()` (line 21) — wrap method body:**

```csharp
public string InsertStudent(Student student)
{
    try
    {
        // ... existing code ...
        return message;
    }
    catch (SqlException ex)
    {
        return "Error: Database error - " + ex.Message;
    }
    catch (Exception ex)
    {
        return "Error: " + ex.Message;
    }
}
```

---

### 4. `Controllers/HomeController.cs`

**A. `InsertEnrollment` POST (line 76-87):**

Current:
```csharp
[HttpPost]
public ActionResult InsertEnrollment(EnrollmentInsertViewModel vm)
{
    EnrollmentDAL da = new EnrollmentDAL();
    string result;
    result = da.SaveEnrollment(vm);
    return Content(result);
}
```

Replace with:
```csharp
[HttpPost]
public ActionResult InsertEnrollment(EnrollmentInsertViewModel vm)
{
    try
    {
        EnrollmentDAL da = new EnrollmentDAL();
        string result = da.SaveEnrollment(vm);
        return Content(result);
    }
    catch (Exception ex)
    {
        return Content("Error: " + ex.Message);
    }
}
```

**B. `DeleteEnrollment` POST (line 98-104):**

Current:
```csharp
[HttpPost]
public JsonResult DeleteEnrollment(int id)
{
    EnrollmentDAL da = new EnrollmentDAL();
    string result = da.DeleteEnrollmentById(id);
    return Json(new { message = result });
}
```

Replace with:
```csharp
[HttpPost]
public JsonResult DeleteEnrollment(int id)
{
    try
    {
        EnrollmentDAL da = new EnrollmentDAL();
        string result = da.DeleteEnrollmentById(id);
        return Json(new { message = result });
    }
    catch (Exception ex)
    {
        return Json(new { message = "Error: " + ex.Message });
    }
}
```

**C. GET `InsertEnrollment` (line 44-73):**

Current — no try-catch around 3+ DAL calls.

Replace with try-catch that returns `PartialView("_EnrollmentForm", emptyVm)` with an error message on failure.

---

### 5. `Controllers/StudentController.cs`

**A. `InsertStudent` POST (line 20-29):**

Current:
```csharp
[HttpPost]
public ContentResult InsertStudent(Student student)
{
    student.CreatedBy = "admin";
    student.LastModifiedBy = "admin";
    string msg = dal.InsertStudent(student);
    return Content(msg);
}
```

Replace with:
```csharp
[HttpPost]
public ContentResult InsertStudent(Student student)
{
    try
    {
        student.CreatedBy = "admin";
        student.LastModifiedBy = "admin";
        string msg = dal.InsertStudent(student);
        return Content(msg);
    }
    catch (Exception ex)
    {
        return Content("Error: " + ex.Message);
    }
}
```

---

### 6. `Views/Shared/_ListData.cshtml` — StudentName substring guard

Two locations (lines 58 and 137 in table view and card view):

Current:
```razor
@(string.IsNullOrEmpty(item.StudentName) ? "S" : item.StudentName.Substring(0, 1).ToUpper())
```

This is already guarded with a null/empty check — safe.

**Additional guard for EnrollmentDate null (lines 68, 154):**

Current:
```razor
@item.EnrollmentDate.ToString("yyyy-MM-dd")
```

Replace with:
```razor
@(item.EnrollmentDate != null ? item.EnrollmentDate.ToString("yyyy-MM-dd") : "")
```
