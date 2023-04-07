using Moq.AutoMock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MultiSpeak_V416_Release.Services
{
    public class MeterReadingService
    {
        private readonly int numberOfMeters;
        private readonly List<MeterReadingProxy> _meterReadingList;

        public MeterReadingService()
        {
            numberOfMeters = 15000;
            var dateRanges = SplitDateRange(RoundUpToNearest30(DateTime.Now.AddMonths(-1)), DateTime.Now, 1440);
            //_meterReadingList = Enumerable.Range(0, 15000)
            //    .SelectMany(meterId => dateRanges.Select((x, i) => new meterReading {
            //        meterID = new meterID {
            //            meterNo = $"M{meterId.ToString().PadLeft(5, '0')}",
            //            utility = "Electric",
            //            objectID = meterId.ToString(),
            //            serviceType = serviceType.Electric,
            //        },
            //        readingValues = new[] {
            //            new readingValue {
            //                measurementPeriod = measurementPeriod.Current,
            //                measurementPeriodSpecified = true,
            //                timeStamp = x.Item1,
            //                timeStampSpecified = true,
            //                units = uom.kWh,
            //                unitsSpecified = true,
            //                value = "60000"
            //            },
            //            new readingValue {
            //                measurementPeriod = measurementPeriod.Current,
            //                measurementPeriodSpecified = true,
            //                timeStamp = x.Item1,
            //                timeStampSpecified = true,
            //                units = uom.kVA,
            //                unitsSpecified = true,
            //                value = ".4"
            //            },
            //            new readingValue {
            //                measurementPeriod = measurementPeriod.Current,
            //                measurementPeriodSpecified = true,
            //                timeStamp = x.Item1,
            //                timeStampSpecified = true,
            //                units = uom.kW,
            //                unitsSpecified = true,
            //                value = "100"
            //            }
            //        }
            //    })).Select((x, i) => {
            //        x.objectID = i.ToString();
            //        var y = new MeterReadingProxy {
            //            Id = i,
            //            meterReading = x
            //        };
            //        return y;
            //    }).ToList();
            _meterReadingList = new List<MeterReadingProxy>();
        }

        private class MeterReadingProxy
        {
            public int Id { get; set; }

            public meterReading meterReading { get; set; }
        }

        public meterReading[] GetLatestReadings(string lastReceived)
        {
            // (DateStart - DateEnd) / 15 minutes = number of time periods
            // n time periods * m meters = number of readings
            // 

            // figure out which time period we're in
            int.TryParse(lastReceived, out int startId);
            startId++;

            int tp = startId / numberOfMeters;
            int count = 50;

            if ((numberOfMeters * (tp + 1)) - startId < count) {
                count = numberOfMeters - startId;
            }

            var timePeriod = RoundUpToNearest30(DateTime.Now.AddMonths(-1)).AddMinutes(tp * 15);

            return Enumerable.Range(startId, count)
                .Select(meterId => new meterReading {
                    objectID = ((tp * numberOfMeters) + (meterId % numberOfMeters)).ToString(),
                    meterID = new meterID {
                        meterNo = $"M{((meterId % numberOfMeters) + (tp * 1)).ToString().PadLeft(5, '0')}",
                        utility = "Electric",
                        objectID = ((meterId % numberOfMeters) + (tp * 1)).ToString(),
                        serviceType = serviceType.Electric,
                        serviceTypeSpecified = true,
                    },
                    readingValues = new[] {
                        new readingValue {
                            measurementPeriod = measurementPeriod.Current,
                            measurementPeriodSpecified = true,
                            timeStamp = timePeriod,
                            timeStampSpecified = true,
                            units = uom.kWh,
                            unitsSpecified = true,
                            value = "60000"
                        },
                        new readingValue {
                            measurementPeriod = measurementPeriod.Current,
                            measurementPeriodSpecified = true,
                            timeStamp = timePeriod,
                            timeStampSpecified = true,
                            units = uom.kVA,
                            unitsSpecified = true,
                            value = ".4"
                        },
                        new readingValue {
                            measurementPeriod = measurementPeriod.Current,
                            measurementPeriodSpecified = true,
                            timeStamp = timePeriod,
                            timeStampSpecified = true,
                            units = uom.kW,
                            unitsSpecified = true,
                            value = "100"
                        }
                    }
                }).ToArray();


            //return _meterReadingList
            //    .Where(x => x.Id > startId)
            //    .Take(500)
            //    .Select(x => x.meterReading).ToArray();
        }

        private static IEnumerable<Tuple<DateTime, DateTime>> SplitDateRange(DateTime start, DateTime end, int minuteChunkSize)
        {
            DateTime chunkEnd;
            while ((chunkEnd = start.AddMinutes(minuteChunkSize)) < end) {
                yield return Tuple.Create(start, chunkEnd);
                start = chunkEnd;
            }
            yield return Tuple.Create(start, end);
        }

        private static DateTime RoundUpToNearest30(DateTime datetime)
        {
            double atMinuteInBlock = datetime.TimeOfDay.TotalMinutes % 30;
            double minutesToAdd = 30 - atMinuteInBlock;
            return datetime.AddMinutes(minutesToAdd);
        }
    }
}