using AutoMapper;
using BankManagement.Data;
using BankManagement.Models;
using BankManagement.ViewModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BankManagement.Controllers
{
    public class TrasnsController : Controller
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment webHostEnvironment;
        public TrasnsController(DataContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            webHostEnvironment = hostEnvironment;
        }
        [HttpGet]
        [Route("trans")]
        public IActionResult Index()
        {
            var userid = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
            var user = _context.UsersTbls.Where(x=> x.UserID == userid).FirstOrDefault();
            if(user.UserTypeID == 1)
            {
                var alltrans = _context.TransTbls.ToList();
                return View(alltrans);
            }
            else
            {
                var alltrans = _context.TransTbls.Where(x => x.Sender == userid || x.Receiver == userid).ToList();
                return View(alltrans);
            }
        }


        [HttpGet]
        [Route("transfer")]
        public IActionResult Transfer()
        {
            if (HttpContext.Session.GetString("Email") == null)
            {
                return RedirectToAction("Login", "Home", new { redirecturl = "transfer" });
            }
            var userid = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
            var userinfo = (from u in _context.UsersTbls
                            join b in _context.UsersBankAccTbls on u.UserID equals b.UserID
                            join t in _context.UserTypeTbls on u.UserTypeID equals t.UserTypeID
                            join s in _context.StatusTbls on b.StatusID equals s.StatusID
                            join a in _context.BankAccTypeTbls on b.BankAccTypeID equals a.BankAccTypeID
                            where u.UserID == userid
                            select new UserProfileVM
                            {
                                UserID = u.UserID,
                                UserName = u.UserName,
                                UserEmail = u.UserEmail,
                                UserPhone = u.UserPhone,
                                FullName = u.FullName,
                                UserAddress = u.UserAddress,
                                NIDNumber = u.NIDNumber,
                                BirthDate = u.BirthDate,
                                PictureLink = u.PictureLink,
                                Password = u.Password,
                                Status = s.Status,
                                BankAccType = a.BankAccType,
                                UserType = t.UserType,
                                BankAccNumber = b.BankAccNumber,
                                BankAccBalance = b.BankAccBalance
                            }).FirstOrDefault();
            return View(userinfo);
        }
        [HttpPost]
        [Route("transfer")]
        public JsonResult Transfer(TransVM model)
        {
            var userid = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
            var userbank = _context.UsersBankAccTbls.Where(x => x.UserID == userid).FirstOrDefault();
            var tobank = _context.UsersBankAccTbls.Where(x => x.BankAccNumber == model.ToBankAccNumber).FirstOrDefault();
            if (tobank == null)
            {
                return Json(new { success = false, message = "Invalid Bank Account Number" });
            }
            else
            {
                if (userbank.BankAccBalance < model.TransAmount)
                {
                    return Json(new { success = false, message = "Insufficient Balance" });
                }
                else
                {
                    userbank.BankAccBalance = userbank.BankAccBalance - model.TransAmount;
                    tobank.BankAccBalance = tobank.BankAccBalance + model.TransAmount;
                    _context.Update(userbank);
                    _context.Update(tobank);
                    var trans = new TransTbl
                    {
                        TransTypeID = 1,
                        UserID = userbank.UserID,
                        FromBankAccID = userbank.BankAccID,
                        ToBankAccID = tobank.BankAccID,
                        TransAmount = model.TransAmount,
                        CreatedAT = DateTime.Now,
                        CreatedBy = userid,
                        TransReason = model.TransReason,
                        FromBankAccNumber = userbank.BankAccNumber,
                        ToBankAccNumber = tobank.BankAccNumber,
                        Sender = userbank.UserID,
                        Receiver = tobank.UserID,
                        Type = "Transfer"
                    };
                    _context.Add(trans);
                    _context.SaveChanges();
                    return Json(new { success = true, message = "Transfer Successfull" });
                }
            }
            //return Json("success");
        }

        [HttpGet]
        [Route("mobilerecharge")]
        public IActionResult MobileRecharge()
        {
            if (HttpContext.Session.GetString("Email") == null)
            {
                return RedirectToAction("Login", "Home", new { redirecturl = "mobilerecharge" });
            }
            var userid = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
            var userinfo = (from u in _context.UsersTbls
                            join b in _context.UsersBankAccTbls on u.UserID equals b.UserID
                            join t in _context.UserTypeTbls on u.UserTypeID equals t.UserTypeID
                            join s in _context.StatusTbls on b.StatusID equals s.StatusID
                            join a in _context.BankAccTypeTbls on b.BankAccTypeID equals a.BankAccTypeID
                            where u.UserID == userid
                            select new UserProfileVM
                            {
                                UserID = u.UserID,
                                UserName = u.UserName,
                                UserEmail = u.UserEmail,
                                UserPhone = u.UserPhone,
                                FullName = u.FullName,
                                UserAddress = u.UserAddress,
                                NIDNumber = u.NIDNumber,
                                BirthDate = u.BirthDate,
                                PictureLink = u.PictureLink,
                                Password = u.Password,
                                Status = s.Status,
                                BankAccType = a.BankAccType,
                                UserType = t.UserType,
                                BankAccNumber = b.BankAccNumber,
                                BankAccBalance = b.BankAccBalance
                            }).FirstOrDefault();
            return View(userinfo);
        }
        [HttpPost]
        [Route("mobilerecharge")]
        public JsonResult MobileRecharge(TransVM model)
        {
            var userid = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
            var userbank = _context.UsersBankAccTbls.Where(x => x.UserID == userid).FirstOrDefault();
            if (model.MobileNumber == null)
            {
                return Json(new { success = false, message = "Invalid Phone Number" });
            }
            else
            {
                if (userbank.BankAccBalance < model.TransAmount)
                {
                    return Json(new { success = false, message = "Insufficient Balance" });
                }
                else if(model.Oparator == "grameenphone")
                {
                    string pattern = @"^017\d{8}$";
                    bool isMatch = Regex.IsMatch(model.MobileNumber, pattern);
                    if(isMatch.Equals(true))
                    {
                        return Json(new { success = true, message = "Transfer Successfull" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Invalid GP phone number. It must start with '017'" });
                    }
                }
                else if (model.Oparator == "banglalink")
                {
                    string pattern = @"^019\d{8}$";
                    bool isMatch = Regex.IsMatch(model.MobileNumber, pattern);
                    if (isMatch.Equals(true))
                    {
                        return Json(new { success = true, message = "Transfer Successfull" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Invalid Banglalink phone number. It must start with '019'" });
                    }
                }
                else if (model.Oparator == "robi")
                {
                    string pattern = @"^018\d{8}$";
                    bool isMatch = Regex.IsMatch(model.MobileNumber, pattern);
                    if (isMatch.Equals(true))
                    {
                        return Json(new { success = true, message = "Transfer Successfull" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Invalid Robi phone number. It must start with '018'" });
                    }
                }
                else if (model.Oparator == "airtel")
                {
                    string pattern = @"^016\d{8}$";
                    bool isMatch = Regex.IsMatch(model.MobileNumber, pattern);
                    if (isMatch.Equals(true))
                    {
                        return Json(new { success = true, message = "Transfer Successfull" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Invalid Airtel phone number. It must start with '016'" });
                    }
                }
                else if (model.Oparator == "teletalk")
                {
                    string pattern = @"^015";
                    bool isMatch = Regex.IsMatch(model.MobileNumber, pattern);
                    if (isMatch)
                    {
                        return Json(new { success = true, message = "Transfer Successfull" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Invalid Tteletalk phone number. It must start with '015'" });
                    }
                }
                else
                {
                    userbank.BankAccBalance = userbank.BankAccBalance - model.TransAmount;
                    _context.Update(userbank);
                    var trans = new TransTbl
                    {
                        TransTypeID = 1,
                        UserID = userid,
                        FromBankAccID = userbank.BankAccID,
                        TransAmount = model.TransAmount,
                        MobileNumber = model.MobileNumber,
                        Oparator = model.Oparator,
                        OparatorType = model.OparatorType,
                        CreatedAT = DateTime.Now,
                        CreatedBy = userid,
                        TransReason = model.TransReason,
                        FromBankAccNumber = userbank.BankAccNumber,
                        ToBankAccNumber = "Operaor",
                        Sender = userbank.UserID,
                        Type = "Recharge"

                    };
                    _context.Add(trans);
                    _context.SaveChanges();
                    
                    return Json(new { success = true, message = "Transfer Successfull" });
                }
            }
        }
        [HttpGet]
        [Route("paybill")]
        public IActionResult ElectryBill()
        {
            var userid = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
            var userinfo = (from u in _context.UsersTbls
                            join b in _context.UsersBankAccTbls on u.UserID equals b.UserID
                            join t in _context.UserTypeTbls on u.UserTypeID equals t.UserTypeID
                            join s in _context.StatusTbls on b.StatusID equals s.StatusID
                            join a in _context.BankAccTypeTbls on b.BankAccTypeID equals a.BankAccTypeID
                            where u.UserID == userid
                            select new UserProfileVM
                            {
                                UserID = u.UserID,
                                UserName = u.UserName,
                                UserEmail = u.UserEmail,
                                UserPhone = u.UserPhone,
                                FullName = u.FullName,
                                UserAddress = u.UserAddress,
                                NIDNumber = u.NIDNumber,
                                BirthDate = u.BirthDate,
                                PictureLink = u.PictureLink,
                                Password = u.Password,
                                Status = s.Status,
                                BankAccType = a.BankAccType,
                                UserType = t.UserType,
                                BankAccNumber = b.BankAccNumber,
                                BankAccBalance = b.BankAccBalance
                            }).FirstOrDefault();
            return View(userinfo);
        }
        [HttpPost]
        [Route("paybill")]
        public JsonResult ElectryBill(ElectricityBillVM electricityBillVM)
        {
            var userid = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
            var userbank = _context.UsersBankAccTbls.Where(x => x.UserID == userid).FirstOrDefault();
            var toAccBank = _context.UsersBankAccTbls.Where(x => x.UserID == 1).FirstOrDefault();
            if (userbank.BankAccBalance < electricityBillVM.Amount)
            {
                return Json(new { success = false, message = "Insufficient Balance" });
            }
            else
            {
                userbank.BankAccBalance -= electricityBillVM.Amount;
                toAccBank.BankAccBalance += electricityBillVM.Amount;

                var payBill = new ElectricityBillTbl
                {
                    Provider = electricityBillVM.Provider,
                    CustomerNumber = electricityBillVM.CustomerNumber,
                    Amount = electricityBillVM.Amount,
                    BankAccNo = userbank.BankAccNumber
                };
                _context.Add(payBill);
                _context.SaveChanges();

                var transaction = new TransTbl
                {
                    TransTypeID = 1,
                    UserID = userid,
                    FromBankAccID = userbank.BankAccID,
                    ToBankAccID = toAccBank.BankAccID,
                    TransAmount = electricityBillVM.Amount,
                    CreatedAT = DateTime.Now,
                    CreatedBy = userid,
                    TransReason = "ElectricIty bill payment",
                    FromBankAccNumber = userbank.BankAccNumber,
                    ToBankAccNumber = "Provider",
                    Sender = userbank.UserID,
                    Type = "Bill Pay"
                };
                _context.Add(transaction);
                _context.SaveChanges();
                return Json(new { success = true, message = "Bill paid. Transfer Successfull" });
            }

        }

        [HttpGet]
        [Route("deposite")]
        public IActionResult Deposite()
        {
            var depositeDetails = _context.DepositeTbls.ToList();
            return View(depositeDetails);
        }

        [HttpPost]
        [Route("deposite")]
        public JsonResult Deposite(TransVM transVM)
        {
            var userid = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
            var userbank = _context.UsersBankAccTbls.Where(x => x.UserID == userid).FirstOrDefault();
            var tobank = _context.UsersBankAccTbls.Where(x => x.BankAccNumber == transVM.ToBankAccNumber).FirstOrDefault();
            if(tobank == null)
            {
                return Json(new { success = false, message = "Invalid Bank Account Number" });
            }
            else
            {
                userbank.BankAccBalance = userbank.BankAccBalance + transVM.TransAmount;
                tobank.BankAccBalance = tobank.BankAccBalance + transVM.TransAmount;
                _context.Update(userbank);
                _context.Update(tobank);
                var trans = new TransTbl
                {
                    TransTypeID = 4,
                    UserID = userid,
                    ToBankAccID = tobank.BankAccID,
                    TransAmount = transVM.TransAmount,
                    CreatedAT = DateTime.Now,
                    CreatedBy = userid,
                    TransReason = "Deposit",
                    FromBankAccNumber = userbank.BankAccNumber,
                    ToBankAccNumber = tobank.BankAccNumber,
                    Sender = userbank.UserID,
                    Receiver = tobank.UserID,
                    Type = "Deposit"
                };
                _context.Add(trans);
                if (_context.SaveChanges() > 0)
                {
                    return Json(new { success = true, message = "Successfully deposited" });
                }
                else
                {
                    return Json(new { success = true, message = "Deposit failed!" });
                }                
            }
            



        }
        [HttpGet]
        [Route("depositinfo")]
        public IActionResult DepositInfo()
        {
            var userid = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
            var userinfo = (from u in _context.UsersTbls
                            join b in _context.UsersBankAccTbls on u.UserID equals b.UserID
                            join t in _context.UserTypeTbls on u.UserTypeID equals t.UserTypeID
                            join s in _context.StatusTbls on b.StatusID equals s.StatusID
                            join a in _context.BankAccTypeTbls on b.BankAccTypeID equals a.BankAccTypeID
                            where u.UserID == userid
                            select new UserProfileVM
                            {
                                UserType = t.UserType,
                                UserTypeId = t.UserTypeID
                                
                            }).FirstOrDefault();
            
            if (userinfo.UserType.Equals("User"))
            {
                var depositeDetails = _context.DepositeTbls.Where(x => x.UserId == userid).ToList();
                return View(depositeDetails);
            }
            else
            {
                return View();
            }
        }
        [HttpGet]
        [Route("fixed-deposit")]
        public IActionResult FixedDeposit()
        {

            return View();
        }

        [HttpPost]
        [Route("fixed-deposit")]
        public JsonResult FixedDeposit(FixedDepositVM fixedDepositVM)
        {
            if (fixedDepositVM == null)
            {
                return Json(new { success = false, message = "Please fullfill the requirements!" });
            }
            else
            {
                //accountno
                string LastBankAccNum = null;
                var BankAccExist = _context.FixedDeposits.ToList();
                if (BankAccExist.Count > 0)
                {
                    LastBankAccNum = _context.UsersBankAccTbls.OrderByDescending(x => x.BankAccID).FirstOrDefault().BankAccNumber;

                }

                if (LastBankAccNum == null)
                {
                    LastBankAccNum = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + "/0000000001";
                }
                else
                {
                    var split = LastBankAccNum.Split('/');
                    var last = split[1];
                    var lastNum = Convert.ToInt32(last);
                    lastNum++;
                    LastBankAccNum = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + "/" + lastNum.ToString("0000000000");
                }

                //
                var userid = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
                var userbank = _context.UsersBankAccTbls.Where(x => x.UserID == userid).FirstOrDefault();
                userbank.BankAccBalance -= fixedDepositVM.InvestAmount;
                _context.Update(userbank);
                _context.SaveChanges();
                //var tobank = _context.UsersBankAccTbls.Where(x => x.BankAccNumber == transVM.ToBankAccNumber).FirstOrDefault();
                DateTime curr = DateTime.Now;
                DateTime maturityDate = curr.AddYears(fixedDepositVM.Period);
                var ma = Convert.ToInt32(fixedDepositVM.InvestAmount * 5.5);
                if(userbank.BankAccBalance > fixedDepositVM.InvestAmount)
                {
                    var fd = new FixedDeposit
                    {
                        DebitAcc = userbank.BankAccNumber,
                        FDAccNo = "fd-" + LastBankAccNum,
                        InvestAmount = fixedDepositVM.InvestAmount,
                        Period = fixedDepositVM.Period,
                        InterestPayout = "On maturity",
                        InterestRate = 0.5,
                        MaturityDate = maturityDate,
                        MaturityAmount = ma,
                        NomineeName = fixedDepositVM.NomineeName,
                        DOB = fixedDepositVM.DOB,
                        UserId = userid
                    };
                    _context.Add(fd);
                    if (_context.SaveChanges() > 0)
                    {
                        return Json(new { success = true, message = "Success! Fixed Deposit Account Created." });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Something went wrong! Please try again" });
                    }

                }
                else
                {
                    return Json(new { success = false, message = "Something went wrong! Please try again" });
                }
            }
        }
        [HttpGet]
        [Route("closefd")]
        public IActionResult CloseFD()
        {
            var userid = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
            var userfd = _context.FixedDeposits.Where(x => x.UserId == userid).FirstOrDefault();
            //var fdList = _context.FixedDeposits.FirstOrDefault(); // Fetch the list of Fixed Deposits
            if(userfd == null)
            {
                var msg = "You do not have any active FD.";
                ViewBag.msg = msg;
                return View();
            }
            ViewBag.fd = userfd;
            return View();

        }

        [HttpPost]
        [Route("closefd")]
        public JsonResult CloseFD(FixedDepositVM fixedDepositVM)
        {
            var userid = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
            var userfd = _context.FixedDeposits.Where(x => x.UserId == userid).FirstOrDefault();
            var userbank = _context.UsersBankAccTbls.Where(x => x.UserID == userid).FirstOrDefault();
            if(userfd.MaturityDate.Equals(DateTime.Now) || userfd.MaturityDate > DateTime.Now)
            {
                userbank.BankAccBalance += userfd.MaturityAmount;
                _context.Update(userbank);
                _context.SaveChanges();
                _context.Remove(userfd);
                _context.SaveChanges();
                return Json(new { success = true, message = "Account closed successfully!" });
            }
            else
            {
                return Json(new { success = false, message = "Something went wrong!" });
            }
        }

        [HttpGet]
        [Route("fixed-deposith")]
        public IActionResult FDHistory()
        {
            var allfd = _context.FixedDeposits.ToList();
            return View(allfd);
        }

        [HttpGet]
        [Route("emi")]
        public IActionResult EMI()
        {
            return View();
        }
        [HttpPost]
        [Route("emi")]
        public IActionResult EMI(decimal principalAmount, decimal interestRate, int loanTenure)
        {
            decimal rate = interestRate / 100 / 12; // Monthly interest rate

            // EMI Calculation
            decimal emi = (principalAmount * rate * (decimal)Math.Pow(1 + (double)rate, loanTenure)) / ((decimal)Math.Pow(1 + (double)rate, loanTenure) - 1);

            decimal totalPayable = emi * loanTenure;
            decimal totalInterestPayable = totalPayable - principalAmount;

            ViewBag.EMI = emi;
            ViewBag.TotalPayable = totalPayable;
            ViewBag.TotalInterestPayable = totalInterestPayable;

            return View();
        }
        [HttpGet]
        [Route("loan")]
        public IActionResult Loan() 
        {
            
            return View(); 
        }
        [HttpPost]
        [Route("loan")]
        public JsonResult Loan(LoanVM loanVM)
        {
            var userid = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
            var userbank = _context.UsersBankAccTbls.Where(x => x.UserID == userid).FirstOrDefault();
            var fee = loanVM.Amount * 0.005;
            var r = 9.0 / 100;
            var tpa = loanVM.Amount + (loanVM.Amount * r);
            if(loanVM.YearlyIncome < 100000)
            {
                return Json(new { success = false, message = "You are not eligible!" });
            }
            else
            {

                var req = new Loan
                {
                    Amount = loanVM.Amount,
                    Period = loanVM.Period,
                    ProcessingFee = fee,
                    ReceivedAmmount = loanVM.Amount - fee,
                    InterestRate = r,
                    TotalPayableAmount = tpa,
                    MonthlyReturnable = tpa / loanVM.Period,
                    LoanType = loanVM.LoanType,
                    YearlyIncome = loanVM.YearlyIncome,
                    ApplicationDate = DateTime.Now,
                    Status = "Processing",
                    Phone = loanVM.Phone,
                    UserId = userid
                };
                _context.Add(req);
                _context.SaveChanges();
                return Json(new { success = true, message = "Request Successfull!" });
            }
        }
        [HttpGet]
        [Route("loanproc")]
        public IActionResult LoanProc()
        {
            var item = _context.Loans.Where(x=>x.Status == "Processing").ToList();
            return View(item);
        }
        [HttpGet]
        [Route("loanprocaction/{response}/{id}")]
        public IActionResult LoanProcAction(string response, int id)
        {
            var item = _context.Loans.Where(x=> x.Id == id).FirstOrDefault();
            if (response.Equals("Approved"))
            {
                item.Status = response;
                _context.Update(item);
                _context.SaveChanges();
            }
            else
            {
                _context.Remove(item);
                _context.SaveChanges();
            }
            
            return RedirectToAction("LoanProc");
        }
    }
}
