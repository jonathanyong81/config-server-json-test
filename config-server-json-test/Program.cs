using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;

namespace config_server_json_test
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ParseJson("config.json").Wait();

            Console.ReadLine();
        }

        private static async Task ParseJson(string fileName)
        {
            var jobTypes = new List<JobType>();
            var supplierConfig = new SupplierConfiguration
            {
                SupplierCode = "FRST",
                JobTypes = jobTypes
            };

            var json = await File.ReadAllTextAsync(fileName).ConfigureAwait(false);

            var deserialized = JObject.Parse(json);

            var jobTypeJProperties = deserialized.Properties();
            foreach (var jt in jobTypeJProperties)
            {
                var jobType = new JobType
                {
                    JobTypeName = jt.Name,
                    FuelTypes = jt.Value.Children<JProperty>().Select(ft => new FuelType
                    {
                        FuelTypeName = ft.Name,
                        MeterTypes = ft.Value.Children<JProperty>().Select(mt => new MeterType
                        {
                            MeterTypeName = mt.Name,
                            ClassificationTypes = mt.Value.Children<JProperty>().Select(ct => new ClassificationType
                            {
                                ClassificationTypeName = ct.Name,
                                BusinessTypes = ct.Value.Children<JProperty>().Select(bt => new BusinessType
                                {
                                    BusinessTypeName = bt.Name,
                                    ExchangeTypes = bt.Value.Children<JProperty>().FirstOrDefault(et => et.Name == "Exchange Types")?.Value?.Children<JProperty>().Select(et => et.Name),
                                    InvestigateTypes = bt.Value.Children<JProperty>().FirstOrDefault(et => et.Name == "Investigate Types")?.Value?.Children<JProperty>().Select(et => et.Name),
                                    RepairTypes = bt.Value.Children<JProperty>().FirstOrDefault(et => et.Name == "Repair Types")?.Value?.Children<JProperty>().Select(et => et.Name)
                                })
                            })
                        })
                    })
                };

                jobTypes.Add(jobType);
            }

            var newJson = JsonConvert.SerializeObject(supplierConfig);
            Console.WriteLine(newJson);

            await File.WriteAllTextAsync("transformed.json", newJson).ConfigureAwait(false);
        }
    }

    class SupplierConfiguration
    {
        public string SupplierCode { get; set; }
        public IEnumerable<JobType> JobTypes { get; set; }
    }

    class JobType
    {
        public string JobTypeName { get; set; }
        public IEnumerable<FuelType> FuelTypes { get; set; }
    }

    class FuelType
    {
        public string FuelTypeName { get; set; }
        public IEnumerable<MeterType> MeterTypes { get; set; }
    }

    class MeterType
    {
        public string MeterTypeName { get; set; }
        public IEnumerable<ClassificationType> ClassificationTypes { get; set; }
    }

    class ClassificationType
    {
        public string ClassificationTypeName { get; set; }
        public IEnumerable<BusinessType> BusinessTypes { get; set; }
    }

    class BusinessType
    {
        public string BusinessTypeName { get; set; }
        public IEnumerable<string> ExchangeTypes { get; set; }
        public IEnumerable<string> InvestigateTypes { get; set; }
        public IEnumerable<string> RepairTypes { get; set; }
    }
}