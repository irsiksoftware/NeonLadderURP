using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NeonLadder.Utilities.Editor
{
    public class BuildReportGenerator : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            string reportPath = "BuildReports/BuildReport.txt";
            System.IO.Directory.CreateDirectory("BuildReports");
            System.IO.File.WriteAllText(reportPath, report.summary.ToString());

            Debug.Log($"Build report saved to {reportPath}");
        }
    }

}
