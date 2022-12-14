using ASACS5.Models.Site;
using ASACS5.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Diagnostics;

namespace ASACS5.Controllers
{
    public class SiteController : Controller
    {
        // GET: Site
        public ActionResult Index()
        {
            // Get the logged in Site ID from the session
            int? SiteID = Session["SiteID"] as int?;

            // if there is none, redirect to the login page
            if (!SiteID.HasValue) return RedirectToAction("Login", "Account");

            // Set up the view model and run SQL to populate the properties
            SiteIndexViewModel vm = new SiteIndexViewModel { SiteID = SiteID.Value };
            object queryResult = null;
            // Determine if site has foodbank
            queryResult = SqlHelper.ExecuteScalar(String.Format("SELECT COUNT(SiteID) FROM foodbank WHERE SiteID = {0}", SiteID.Value));
            if (queryResult != null && int.Parse(queryResult.ToString()) > 0) vm.HasFoodBank = true;
            // Determine if site has foodpantry
            queryResult = SqlHelper.ExecuteScalar(String.Format("SELECT COUNT(SiteID) FROM foodpantry WHERE SiteID = {0}", SiteID.Value));
            if (queryResult != null && int.Parse(queryResult.ToString()) > 0) vm.HasFoodPantry = true;
            // Determine if site has shelter
            queryResult = SqlHelper.ExecuteScalar(String.Format("SELECT COUNT(SiteID) FROM shelter WHERE SiteID = {0}", SiteID.Value));
            if (queryResult != null && int.Parse(queryResult.ToString()) > 0) vm.HasShelter = true;
            // Determine if site has soupkitchen
            queryResult = SqlHelper.ExecuteScalar(String.Format("SELECT COUNT(SiteID) FROM soupkitchen WHERE SiteID = {0}", SiteID.Value));
            if (queryResult != null && int.Parse(queryResult.ToString()) > 0) vm.HasSoupKitchen = true;

            return View(vm);
        }

        public ActionResult FoodBank()
        {
            // Get the logged in Site ID from the session
            int? SiteID = Session["SiteID"] as int?;

            // if there is none, redirect to the login page
            if (!SiteID.HasValue) return RedirectToAction("Login", "Account");

            FoodBankViewModel vm = new FoodBankViewModel();


            // find out if we have a food bank for this site already or not
            string sql = String.Format("SELECT COUNT(*) FROM foodbank WHERE SiteID = {0}; ", SiteID.Value.ToString());

            vm.FoodBankExists = Int32.Parse(SqlHelper.ExecuteScalar(sql).ToString()) > 0;

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FoodBank(FoodBankViewModel vm)
        {
            // insert a new Food Bank based on the logged in users Site ID
            int? SiteID = Session["SiteID"] as int?;

            // Insert into foodbank table the user's Site ID
            string sql = String.Format(
                "INSERT INTO foodbank (SiteID) VALUES ({0}); ",  SiteID.Value.ToString());

            SqlHelper.ExecuteNonQuery(sql);

            vm.SiteID = SiteID.Value; // set the ID since it now exists
            vm.StatusMessage = "Succesfully added!";            

            return View(vm);
        }

        public ActionResult SoupKitchen()
        {
            // Get the logged in Site ID from the session
            int? SiteID = Session["SiteID"] as int?;

            // if there is none, redirect to the login page
            if (!SiteID.HasValue) return RedirectToAction("Login", "Account");

            // set up the response object
            SoupKitchenViewModel vm = new SoupKitchenViewModel();

            // Find soup kitchen for given Site ID
            string sql = String.Format(
                "SELECT TotalSeatsAvailable, RemainingSeatsAvailable, HoursOfOperation, ConditionsForUse, Description " +
                "FROM soupkitchen WHERE SiteID = {0}; ", SiteID.Value.ToString());

            // run the sql against the db
            object[] result = SqlHelper.ExecuteSingleSelect(sql, 5);

            // if we got a result, populate the view model fields
            if (result != null)
            {
                vm.SiteID = SiteID.Value;
                vm.TotalSeatsAvailable = int.Parse(result[0].ToString());
                vm.RemainingSeatsAvailable = int.Parse(result[1].ToString());
                vm.HoursOfOperation = result[2].ToString();
                vm.ConditionsForUse = result[3].ToString();
                vm.Description = result[4].ToString();
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SoupKitchen(SoupKitchenViewModel vm)
        {
            if (ModelState.IsValid)
            {
                // this is needed for some reason.. come back to it
                // http://stackoverflow.com/questions/4837744/hiddenfor-not-getting-correct-value-from-view-model
                ModelState.Remove("SiteID");

                // find out if the soup kitchen exists for this SiteID already
                // and set up the SQL to INSERT or UPDATE accordingly

                if (vm.SiteID.Equals(0))
                {
                    // we didn't find an existing soup kitchen. so insert a new one based on the logged in users Site ID
                    int? SiteID = Session["SiteID"] as int?;
                    // Add soup kitchen for the user's site
                    string sql = String.Format(
                        "INSERT INTO soupkitchen (SiteID, TotalSeatsAvailable, RemainingSeatsAvailable, HoursOfOperation, ConditionsForUse, Description) " +
                        "VALUES ({0}, {1}, {2}, '{3}', '{4}', '{5}'); ",
                        SiteID.Value.ToString(), vm.TotalSeatsAvailable, vm.RemainingSeatsAvailable, vm.HoursOfOperation, vm.ConditionsForUse, vm.Description);

                    SqlHelper.ExecuteNonQuery(sql);

                    vm.SiteID = SiteID.Value; // set the ID since it now exists
                    vm.StatusMessage = "Succesfully added!";
                }
                else
                {
                    // update the existing record 

                    string sql = String.Format(
                        "UPDATE soupkitchen " +
                        "SET TotalSeatsAvailable = {0}, " +
                        "RemainingSeatsAvailable = {1}, " +
                        "HoursOfOperation = '{2}', " +
                        "ConditionsForUse = '{3}', " +
                        "Description = '{4}' " +
                        "WHERE SiteID = {5}; ",
                        vm.TotalSeatsAvailable, vm.RemainingSeatsAvailable, vm.HoursOfOperation, vm.ConditionsForUse, vm.Description, vm.SiteID);

                    SqlHelper.ExecuteNonQuery(sql);

                    vm.StatusMessage = "Succesfully updated!";
                }

            }
            return View(vm);
        }

		public ActionResult FoodPantry()
		{
			// Get the logged in Site ID from the session
			int? SiteID = Session["SiteID"] as int?;

			// if there is none, redirect to the login page
			if (!SiteID.HasValue) return RedirectToAction("Login", "Account");

			// set up the response object
			FoodPantryViewModel vm = new FoodPantryViewModel();

			// Find food pantry for user's site
			string sql = String.Format(
                "SELECT HoursOfOperation, ConditionsForUse, Description " +
				"FROM foodpantry WHERE SiteID = {0}; ", SiteID.Value.ToString());

			// run the sql against the db
			object[] result = SqlHelper.ExecuteSingleSelect(sql, 3);

			// if we got a result, populate the view model fields
			if (result != null)
			{
				vm.SiteID = SiteID.Value;
				vm.HoursOfOperation = result[0].ToString();
				vm.ConditionsForUse = result[1].ToString();
                vm.Description = result[2].ToString();

            }

			return View(vm);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult FoodPantry(FoodPantryViewModel vm)
		{
			if (ModelState.IsValid)
			{
				// this is needed for some reason.. come back to it
				// http://stackoverflow.com/questions/4837744/hiddenfor-not-getting-correct-value-from-view-model
				ModelState.Remove("SiteID");

				// find out if the soup kitchen exists for this SiteID already
				// and set up the SQL to INSERT or UPDATE accordingly

				if (vm.SiteID.Equals(0))
				{
					// we didn't find an existing soup kitchen. so insert a new one based on the logged in users Site ID
					int? SiteID = Session["SiteID"] as int?;
                    // Insrt food pantry for user's site
					string sql = String.Format(
                        "INSERT INTO foodpantry (SiteID, HoursOfOperation, ConditionsForUse, Description) " +
						"VALUES ({0}, '{1}', '{2}', '{3}'); ",
						SiteID.Value.ToString(), vm.HoursOfOperation, vm.ConditionsForUse, vm.Description);

					SqlHelper.ExecuteNonQuery(sql);

					vm.SiteID = SiteID.Value; // set the ID since it now exists
					vm.StatusMessage = "Succesfully added!";
				}
				else
				{
					// update the existing record 

					string sql = String.Format(
						"UPDATE foodpantry " +
						"SET HoursOfOperation = '{0}', " +
						"ConditionsForUse = '{1}', " +
                        "Description = '{2}' " +
                        "WHERE SiteID = {3}; ",
						 vm.HoursOfOperation, vm.ConditionsForUse, vm.Description, vm.SiteID
					);

					SqlHelper.ExecuteNonQuery(sql);

					vm.StatusMessage = "Succesfully updated!";
				}

			}
			return View(vm);
		}
		
		public ActionResult Shelter()
        {
            // Get the logged in Site ID from the session
            int? SiteID = Session["SiteID"] as int?;

            // if there is none, redirect to the login page
            if (!SiteID.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            // set up the response object
            ShelterViewModel vm = new ShelterViewModel();

            // find shelter's for user's site
            string sql = String.Format(
                "SELECT MaleBunksAvailable, FemaleBunksAvailable, MixedBunksAvailable, RoomsAvailable, HoursOfOperation, ConditionsForUse, Description " +
                "FROM shelter WHERE SiteID = {0}; ", SiteID.Value.ToString());

            // run the sql against the db
            object[] result = SqlHelper.ExecuteSingleSelect(sql, 7);

            // if we got a result, populate the view model fields
            if (result != null)
            {
                vm.SiteID = SiteID.Value;
                vm.MaleBunksAvailable = int.Parse(result[0].ToString());
                vm.FemaleBunksAvailable = int.Parse(result[1].ToString());
                vm.MixedBunksAvailable = int.Parse(result[2].ToString());
                vm.RoomsAvailable = int.Parse(result[3].ToString());
                vm.HoursOfOperation = result[4].ToString();
                vm.ConditionsForUse = result[5].ToString();
                vm.Description = result[6].ToString();
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Shelter(ShelterViewModel vm)
        {
            if (ModelState.IsValid)
            {
                // this is needed for some reason.. come back to it
                // http://stackoverflow.com/questions/4837744/hiddenfor-not-getting-correct-value-from-view-model
                ModelState.Remove("SiteID");

                // find out if the soup kitchen exists for this SiteID already
                // and set up the SQL to INSERT or UPDATE accordingly

                if (vm.SiteID.Equals(0))
                {
                    // we didn't find an existing soup kitchen. so insert a new one based on the logged in users Site ID
                    int? SiteID = Session["SiteID"] as int?;
                    //insert shelter for site
                    string sql = String.Format(
                        "INSERT INTO shelter (SiteID, MaleBunksAvailable, FemaleBunksAvailable, MixedBunksAvailable, RoomsAvailable, HoursOfOperation, ConditionsForUse, Description) " +
                        "VALUES ({0}, {1}, {2}, {3}, {4}, '{5}', '{6}', '{7}'); ",
                        SiteID.Value.ToString(), vm.MaleBunksAvailable, vm.FemaleBunksAvailable, vm.MixedBunksAvailable, vm.RoomsAvailable, vm.HoursOfOperation, vm.ConditionsForUse, vm.Description);
                    
                    SqlHelper.ExecuteNonQuery(sql);

                    vm.SiteID = SiteID.Value; // set the ID since it now exists
                    vm.StatusMessage = "Succesfully added!";
                }
                else
                {
                    // update the existing record 

                    string sql = String.Format(
                        "UPDATE shelter " +
                        "SET MaleBunksAvailable = {0}, " +
                        "FemaleBunksAvailable = {1}, " +
                        "MixedBunksAvailable = {2}, " +
                        "RoomsAvailable = {3}, " +
                        "HoursOfOperation = '{4}', " +
                        "ConditionsForUse = '{5}', " +
                        "Description = '{6}' " +
                        "WHERE SiteID = {7} ;",
                        vm.MaleBunksAvailable, vm.FemaleBunksAvailable, vm.MixedBunksAvailable, vm.RoomsAvailable, vm.HoursOfOperation, vm.ConditionsForUse, vm.Description, vm.SiteID);

                    SqlHelper.ExecuteNonQuery(sql);

                    vm.StatusMessage = "Succesfully updated!";
                }

            }
            return View(vm);
        }

        [Route("Site/DeleteService/{serviceType}")]
        public ActionResult DeleteService(string serviceType)
        {
            // Get the logged in Site ID from the session
            int? SiteID = Session["SiteID"] as int?;

            // if there is none, they can't do anything so redirect to the login page
            if (!SiteID.HasValue) return RedirectToAction("Login", "Account");

            // start setting up the view model
            DeleteServiceViewModel vm = new DeleteServiceViewModel();
            vm.SiteID = SiteID.Value;

            // now find out if this siteID has a service of the specified type
            string sql = null;
            // determine if service is found at site
            switch (serviceType.ToLower())
            {
                case "foodbank":
                    sql = String.Format("SELECT COUNT(SiteID) FROM foodbank WHERE SiteID = {0}", SiteID.Value);
                    vm.ServiceType = "Food Bank";
                    break;
                case "foodpantry":
                    sql = String.Format("SELECT COUNT(SiteID) FROM foodpantry WHERE SiteID = {0}", SiteID.Value);
                    vm.ServiceType = "Food Pantry";
                    break;
                case "shelter":
                    sql = String.Format("SELECT COUNT(SiteID) FROM shelter WHERE SiteID = {0}", SiteID.Value);
                    vm.ServiceType = "Shelter";
                    break;
                case "soupkitchen":
                    sql = String.Format("SELECT COUNT(SiteID) FROM soupkitchen WHERE SiteID = {0}", SiteID.Value);
                    vm.ServiceType = "Soup Kitchen";
                    break;
                default:
                    throw new Exception("SiteController.DeleteService: non-supported serviceType");
            }

            int count = int.Parse(SqlHelper.ExecuteScalar(sql).ToString());

            if (count > 0)
            {
                vm.ServiceExists = true;
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Site/DeleteService/{serviceType}")]
        public ActionResult DeleteService(string serviceType, DeleteServiceViewModel vm)
        {
            // first thing need is to see if this is the last service for the given Site. if
            // so the user is not allowed to delete it so show an error message.

            string sql = String.Format(
                "SELECT COUNT(comb.SiteID) from " +
                "(SELECT fb.SiteID FROM foodbank fb WHERE fb.SiteID = {0} UNION ALL " +
                " SELECT fp.SiteID FROM foodpantry fp WHERE fp.SiteID = {0} UNION ALL " +
                " SELECT s.SiteID FROM shelter s WHERE s.SiteID = {0} UNION ALL " +
                " SELECT sk.SiteID FROM soupkitchen sk WHERE sk.SiteID = {0}) comb; ", vm.SiteID);

            int numberOfServices = Int32.Parse(SqlHelper.ExecuteScalar(sql).ToString());

            if (numberOfServices.Equals(1))
            {
                vm.ServiceExists = true;
                vm.ErrorMessage = "Error: The last service for a site cannot be deleted. Please contact the system admin to delete your site.";
            }
            else
            { 
                string sql2 = null;

                switch (serviceType.ToLower())
                {
                    case "foodbank":
                        sql2 = String.Format("DELETE FROM foodbank WHERE SiteID = {0}", vm.SiteID);
                        break;
                    case "foodpantry":
                        sql2 = String.Format("DELETE FROM foodpantry WHERE SiteID = {0}", vm.SiteID);
                        break;
                    case "shelter":
                        sql2 = String.Format("DELETE FROM shelter WHERE SiteID = {0}", vm.SiteID);
                        break;
                    case "soupkitchen":
                        sql2 = String.Format("DELETE FROM soupkitchen WHERE SiteID = {0}", vm.SiteID);
                        break;
                    default:
                        throw new Exception("SiteController.DeleteService: non-supported serviceType");
                }

                SqlHelper.ExecuteNonQuery(sql2);

                vm.DeleteCompleted = true;
            }

            return View(vm);
        }
    }
}