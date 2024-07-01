using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeIntern.Utility
{
    public static class SD
    {
        public const string Role_Admin = "Admin";
        public const string Role_Company = "Company";
        public const string Role_Student = "Student";

        public const string CvPath = "C:\\CodeInternUserProfiles\\";

        public static List<string> Position = new List<string>
        {
            "Software Developer",
            "Data Scientist",
            "Product Manager",
            "Systems Analyst",
            "UI/UX Designer",
            "DevOps Engineer",
            "Quality Assurance Engineer",
            "Database Administrator",
            "Network Engineer",
            "IT Project Manager"
        };
        
        public static string[] ProgLanguage = {
            "Python",
            "JavaScript",
            "Java",
            "C++",
            "C#",
            "Ruby",
            "Swift",
            "Go",
            "PHP",
            "Kotlin"
        };

        public static string[] WorkPlace = {
            "Office",
            "Remote",
            "Hybrid"
        };


        public static string[] Technology = {
            "React",
            "Angular",
            "Node.js",
            "Docker",
            "Kubernetes",
            "TensorFlow",
            "AWS (Amazon Web Services)",
            "Azure",
            "MongoDB",
            "Git",
            ".NET",
            "Spring Boot" 
        };

        public static List<string> Cities = new List<string>
        {
            "Zagreb",
            "Split",
            "Rijeka",
            "Osijek",
            "Zadar",
            "Slavonski Brod",
            "Pula",
            "Šibenik",
            "Dubrovnik",
            "Varaždin"
        };

    }

    
}
