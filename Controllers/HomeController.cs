using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VSCODEPROJECT.Models;
using Newtonsoft.Json;



namespace VSCODEPROJECT.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;


    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    static string USER_NAME = "(Please Enter Login!)"; // Omit this part after set the login page as main page
    static string USER_NAME_TAG = "";

    static string SELECTED_DATE = DateTime.Now.ToString("-yyyy-MM");

    static bool saveFile = false;

    public static string getCurrentUser()
    {
        return USER_NAME;
    }

    public static string getCurrentUserTag()
    {
        return USER_NAME_TAG;
    }

    public static string getDate()
    {
        return SELECTED_DATE.Substring(1, SELECTED_DATE.Length - 1);
    }

    public static void saveJsonFileForProject(ProjectListView project_list)
    {

        try
        {
            using (FileStream fs = System.IO.File.Open("C:/Users/dell/VSCODEPROJECT/Activities/activity.json", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                lock (fs)
                {
                    fs.SetLength(0);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }


        string json_data = JsonConvert.SerializeObject(project_list, Formatting.Indented);

        // her kayıt yaparken json file ı komple sil ve tekrar yaz.
        string fileName = "activity.json";



        using (StreamWriter writer = new StreamWriter("C:/Users/dell/VSCODEPROJECT/Activities/" + fileName, true))
        {
            writer.Write(json_data);
        }
    }

    public static ProjectListView readJsonFileForProject()
    {
        var project_list = new ProjectListView();
        try
        {
            using (StreamReader r = new StreamReader("C:/Users/dell/VSCODEPROJECT/Activities/activity.json"))
            {
                string json = r.ReadToEnd();

                project_list = JsonConvert.DeserializeObject<ProjectListView>(json);
            }


        }
        catch
        {
            saveJsonFileForProject(project_list); // if activity file didn't created come here
        }

        return project_list;
    }

    public static void saveJsonFileForActivity(ActivityListView activity_list)
    {
        string current_date = DateTime.Now.ToString("-yyyy-MM");
        string file_name = USER_NAME + SELECTED_DATE + ".json";
        if (saveFile)
        {
            try
            {
                using (FileStream fs = System.IO.File.Open("C:/Users/dell/VSCODEPROJECT/UserActivities/" + file_name, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    lock (fs)
                    {
                        fs.SetLength(0);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            string json_data = JsonConvert.SerializeObject(activity_list, Formatting.Indented);

            // her kayıt yaparken json file ı komple sil ve tekrar yaz.


            using (StreamWriter writer = new StreamWriter("C:/Users/dell/VSCODEPROJECT/UserActivities/" + file_name, true))
            {
                writer.Write(json_data);
            }
        }

    }

    public static ActivityListView readJsonFileForActivity()
    {
        string current_date = DateTime.Now.ToString("-yyyy-MM");
        string file_name = USER_NAME + SELECTED_DATE + ".json";

        var activity_list = new ActivityListView();

        try
        {
            using (StreamReader r = new StreamReader("C:/Users/dell/VSCODEPROJECT/UserActivities/" + file_name)) //format will be => current_user-year-month.json
            {
                string json = r.ReadToEnd();

                activity_list = JsonConvert.DeserializeObject<ActivityListView>(json);
            }

        }
        catch (Exception ex)
        {
            saveJsonFileForActivity(activity_list);
        }

        return activity_list;
    }

    [HttpPost]
    public ActionResult getEditActivity(ActivityWithId editableActivities)
    {
        saveFile = true;

        ViewBag.showEditAlertMessageMissingParts = true;

        int indexNumber = editableActivities.Id;

        string code = editableActivities.Code;

        string date = editableActivities.Date;

        int time = editableActivities.Time;

        string description = editableActivities.Description;

        ViewBag.showEditAlertMessageDate = false;

        if (date != null && time != 0 && description != null)
        {

            string dateSub = "-" + date.Substring(0, 7);

            ViewBag.showEditAlertMessageDate = true;

            if (SELECTED_DATE == dateSub)
            {
                ActivityListView new_activity_list = readJsonFileForActivity();

                new_activity_list.Entries.RemoveAt(indexNumber - 1);

                new_activity_list.Entries.Insert(indexNumber - 1, new Activities() { Code = code, Date = date, Time = time, Description = description });

                saveJsonFileForActivity(new_activity_list);

                var modelForActivities = new ExtraActivitiyListView();

                modelForActivities.currentDayActivityList = getCurrentDayActivities();
                modelForActivities.totalTimeSpentList = getTotalTimeSpentActivities();

                return View("Activities", modelForActivities);
            }
        }

        ViewBag.showEditAlertMessage = true;

        return View("EditActivity", editableActivities);

    }

    [HttpPost]
    public ActionResult getDeleteActivity(IndexId deletedActivity)
    {
        saveFile = true;

        //delete the activity which belongs to selected id and save this file with same file

        int indexNumber = deletedActivity.Id;

        ActivityListView new_activity_list = readJsonFileForActivity();

        new_activity_list.Entries.RemoveAt(indexNumber - 1);

        saveJsonFileForActivity(new_activity_list);


        var modelForActivities = new ExtraActivitiyListView();

        modelForActivities.currentDayActivityList = getCurrentDayActivities();
        modelForActivities.totalTimeSpentList = getTotalTimeSpentActivities();

        return View("Activities", modelForActivities);
    }

    //getDetails funciton is here

    [HttpPost]
    public ActionResult getSelectDayForActivities(Date date)
    {
        saveFile = false;

        string selectedDate = date.selectedDate;


        if (selectedDate == null)
        {
            SELECTED_DATE = DateTime.Now.ToString("-yyyy-MM");
        }
        else
        {
            SELECTED_DATE = "-" + selectedDate;
        }

        var modelForActivities = new ExtraActivitiyListView();

        modelForActivities.currentDayActivityList = getCurrentDayActivities();
        modelForActivities.totalTimeSpentList = getTotalTimeSpentActivities();

        return View("Activities", modelForActivities);
    }

    [HttpPost]
    public ActionResult getSelectDayForReports(Date date)
    {
        saveFile = false;

        string selectedDate = date.selectedDate;


        if (selectedDate == null)
        {
            SELECTED_DATE = DateTime.Now.ToString("-yyyy-MM");
        }
        else
        {
            SELECTED_DATE = "-" + selectedDate;
        }

        Console.WriteLine(SELECTED_DATE);

        var modelForReports = new ReportListView();

        modelForReports.reportList = getReports();

        return View("Reports", modelForReports);
    }

    [HttpPost]
    public ActionResult getNewActivity(Activities newActivity)
    {

        string date = newActivity.Date;
        int time = newActivity.Time;
        string description = newActivity.Description;

        var model = new ActivityListView();

        if (date != null && time != 0 && description != null)
        {
            SELECTED_DATE = "-" + newActivity.Date.Substring(0, 7);
        }

        model.Entries = getActivities();

        if (date != null && time != 0 && description != null)
        {
            saveFile = true;

            model.Entries.Add(newActivity);
        }


        saveJsonFileForActivity(model);


        var modelForActivities = new ExtraActivitiyListView();

        modelForActivities.currentDayActivityList = getCurrentDayActivities();
        modelForActivities.totalTimeSpentList = getTotalTimeSpentActivities();

        return View("Activities", modelForActivities);

    }

    [HttpPost]
    public ActionResult getNewProject(Project newProject)
    {

        // take receivedModel and create new project on activity.json (Add getActiveProject() List)
        string project_code = newProject.Code;
        string project_manager = newProject.Manager;
        string project_name = newProject.Name;
        int project_budget = newProject.Budget;

        ViewBag.Message = project_code;

        var model = new ProjectListView();
        model.Activities = getProjects();

        bool projectHasValidCode = false;

        if (project_code != null && project_manager != null && project_name != null)
        {
            projectHasValidCode = true;

            for (int i = 0; i < model.Activities.Count(); i++)
            {
                if (model.Activities[i].Code == project_code)
                {
                    projectHasValidCode = false; // don't add new project to projectListView
                }
            }

        }

        if (projectHasValidCode)
        {
            model.Activities.Add(newProject);
        }

        saveJsonFileForProject(model);

        // I receive data but I have to return some view but I just return(comeback) to my Index page.
        return View("Projects", model);
    }

    public static List<Activities> getActivities()
    {
        List<Activities> activity_list = new List<Activities>();

        ActivityListView entries_list = readJsonFileForActivity();

        string code, date, description;
        int time;
        try
        {
            for (int i = 0; i < entries_list.Entries.Count(); i++)
            {
                code = entries_list.Entries[i].Code;
                date = entries_list.Entries[i].Date;
                time = entries_list.Entries[i].Time;
                description = entries_list.Entries[i].Description;

                activity_list.Add(new Activities() { Code = code, Date = date, Time = time, Description = description });
            }
        }
        catch (Exception ex)
        {
            // If code is here do nothing new file created and we fill on the create new activity page
        }

        return activity_list;
    }
    public static List<Project> getProjects()
    {
        //Jsondan veri al: activity.json dosyasını bul ve onun içindeki her şeyi oku.

        List<Project> project_list = new List<Project>();

        ProjectListView activities_list = readJsonFileForProject();

        string code, manager, name;
        int budget;
        try
        {
            for (int i = 0; i < activities_list.Activities.Count(); i++)
            {
                code = activities_list.Activities[i].Code;
                manager = activities_list.Activities[i].Manager;
                name = activities_list.Activities[i].Name;
                budget = activities_list.Activities[i].Budget;

                project_list.Add(new Project() { Code = code, Manager = manager, Name = name, Budget = budget });
            }
        }
        catch(Exception ex)
        {
            // If code is here do nothing 
        }


        return project_list;
    }

    public List<Details> getDetails()
    {
        List<Details> detail_list = new List<Details>();

        ActivityListView total_time_spent_activities_list = readJsonFileForActivity();

        ProjectListView project_list = readJsonFileForProject();

        Project project;
        string code, date, desription, dateSub;
        int time;

        try
        {
            ViewBag.haveDetails = true;

            for (int i = 0; i < total_time_spent_activities_list.Entries.Count(); i++)
            {
                date = total_time_spent_activities_list.Entries[i].Date;
                dateSub = date.Substring(8, 2);
                code = total_time_spent_activities_list.Entries[i].Code;
                time = total_time_spent_activities_list.Entries[i].Time;
                desription = total_time_spent_activities_list.Entries[i].Description;

                for (int j = 0; j < project_list.Activities.Count(); j++)
                {

                    if (code == project_list.Activities[j].Code)
                    {
                        project = project_list.Activities[j];
                        detail_list.Add(new Details() { Date = dateSub, Project = project, Time = time, Description = desription });
                        break;
                    }
                }

            }
        }
        catch
        {
            ViewBag.haveDetails = false;
            ViewBag.detailsMessage = "You have no any details for this month";
        }


        return detail_list;
    }

    public List<Reports> getReports()
    {
        // read all directory about users
        List<Reports> report_list = new List<Reports>();

        List<Activities> total_time_spent_activities = new List<Activities>();

        ActivityListView total_time_spent_activities_list = readJsonFileForActivity();

        ProjectListView project_list = readJsonFileForProject();

        string code;
        int time, budget;
        bool findDuplicate;

        try
        {
            ViewBag.haveReports = true;

            for (int i = 0; i < total_time_spent_activities_list.Entries.Count(); i++)
            {
                findDuplicate = false;

                code = total_time_spent_activities_list.Entries[i].Code;
                time = total_time_spent_activities_list.Entries[i].Time;

                for (int j = 0; j < report_list.Count(); j++)
                {

                    if (code == report_list[j].Code)
                    {

                        report_list[j].Time += total_time_spent_activities_list.Entries[i].Time;
                        findDuplicate = true;
                    }
                }

                if (!findDuplicate)
                {
                    report_list.Add(new Reports() { Code = code, Time = time });
                }

            }

            //add project budgets to report_list

            for (int i = 0; i < report_list.Count(); i++)
            {
                code = report_list[i].Code;
                time = report_list[i].Time;

                for (int j = 0; j < project_list.Activities.Count(); j++)
                {
                    if (project_list.Activities[j].Code == report_list[i].Code)
                    {
                        budget = project_list.Activities[j].Budget;

                        report_list.RemoveAt(i);
                        report_list.Insert(i, new Reports() { Code = code, Budget = budget, Time = time });
                    }
                }
            }

        }
        catch
        {
            ViewBag.haveReports = false;
            ViewBag.reportsMessage = "You have no any report for this month";
        }

        return report_list;
    }

    public List<Activities> getCurrentDayActivities()
    {
        List<Activities> currrent_day_activities = new List<Activities>();

        string current_date = DateTime.Now.ToString("yyyy-MM-dd");
        ActivityListView current_day_activities_list = readJsonFileForActivity();

        string code, date, description, dateSub;
        int time;

        try
        {
            ViewBag.haveCurrentDayActivities = false;
            ViewBag.currentDayMessage = "You have no any activity for today";

            for (int i = 0; i < current_day_activities_list.Entries.Count(); i++)
            {

                if (current_date == current_day_activities_list.Entries[i].Date)
                {

                    code = current_day_activities_list.Entries[i].Code;
                    date = current_day_activities_list.Entries[i].Date;
                    time = current_day_activities_list.Entries[i].Time;
                    description = current_day_activities_list.Entries[i].Description;

                    dateSub = date.Substring(8, 2);

                    ViewBag.haveCurrentDayActivities = true;

                    currrent_day_activities.Add(new Activities() { Code = code, Date = dateSub, Time = time, Description = description });
                }

            }
        }
        catch
        {
            ViewBag.haveCurrentDayActivities = false;
            ViewBag.currentDayMessage = "You have no any activity for today";
        }


        return currrent_day_activities;
    }


    public List<Activities> getTotalTimeSpentActivities()
    {
        List<Activities> total_time_spent_activities = new List<Activities>();

        ActivityListView total_time_spent_activities_list = readJsonFileForActivity();

        string code, date, description, dateSub;
        int time;


        try
        {
            ViewBag.haveTotalTimeSpentActivities = true;

            for (int i = 0; i < total_time_spent_activities_list.Entries.Count(); i++)
            {
                date = total_time_spent_activities_list.Entries[i].Date;
                code = total_time_spent_activities_list.Entries[i].Code;
                time = total_time_spent_activities_list.Entries[i].Time;
                description = total_time_spent_activities_list.Entries[i].Description;

                dateSub = date.Substring(8, 2);

                total_time_spent_activities.Add(new Activities() { Code = code, Date = dateSub, Time = time, Description = description });

            }
        }
        catch
        {
            ViewBag.haveTotalTimeSpentActivities = false;
            ViewBag.totalTimeSpentMessage = "You have no any activity for this month";
        }

        return total_time_spent_activities;
    }

    public ActionResult ComboBox()
    {
        var projectList = getProjects();

        return View();
    }

    public IActionResult Index()
    {
        // set initial page
        // check the user name if it is empty ask again to user
        // carry user name to the Main Page

        ViewBag.haveUserName = true;

        return View();
    }

    public IActionResult Activities()
    {
        var model = new ExtraActivitiyListView();
        model.currentDayActivityList = getCurrentDayActivities();
        model.totalTimeSpentList = getTotalTimeSpentActivities();

        return View(model);
    }

    public IActionResult Reports()
    {

        // Show activity datas with Project information
        var model = new ReportListView();

        model.reportList = getReports();

        return View(model);
    }

    public IActionResult Projects()
    {

        var model = new ProjectListView();
        model.Activities = getProjects();

        return View(model);

    }

    [HttpPost]
    public ActionResult Subscribe(User model)
    {
        string user = model.UserName;

        ViewBag.haveUserName = false;

        if (user != null)
        {
            ViewBag.haveUserName = true;

            var model_index = new ProjectListView();
            model_index.Activities = getProjects();

            USER_NAME = user;
            USER_NAME_TAG = " (User)   ";


            for (int i = 0; i < model_index.Activities.Count(); i++)
            {
                if (model_index.Activities[i].Manager == user)
                {
                    USER_NAME = user;
                    USER_NAME_TAG = " (Manager)   ";
                }
            }

            SELECTED_DATE = DateTime.Now.ToString("-yyyy-MM"); // reset date when new user enter-

            return View("Projects", model_index);
        }

        return View("Index", model);

    }

    public IActionResult CreateNewProject()
    {
        ViewBag.showNewProjectAlertMessage = false;

        return View();
    }

    public IActionResult CreateNewActivity()
    {
        ViewBag.showNewActivityAlertMessage = false;

        return View();
    }

    public IActionResult EditActivity()
    {
        ViewBag.showEditAlertMessageMissingParts = false;
        ViewBag.showEditAlertMessageDate = false;

        return View();
    }

    public IActionResult DeleteActivity()
    {


        return View();
    }

    public IActionResult SelectDayForActivities()
    {

        return View();
    }

    public IActionResult selectDayForReports()
    {


        return View();
    }

    public IActionResult Details()
    {
        var model = new DetailListView();

        model.detailList = getDetails();

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
