using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ElasticSearchPatchGenaration.Models;
using Nest;
using QueryEditor.Models;
using QueryEditor.Models.Search;
using QueryEditor.Services.ElasticSearch;

namespace QueryEditor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ElasticClient elasticClient = ElasticSearchService.GetElasticClient();

            ElasticSearchService searchService = new ElasticSearchService();

            //var scriptLE = PatchScriptMetadata.GetPatchScriptMetadata(new List<FieldPatchDescriptor>
            //    {
            //        new FieldPatchDescriptor
            //        {
            //            FieldPath = "lastEngagements",
            //            FieldPatchType = FieldPatchTypes.Append,
            //            PatchValue = new SampleObject(),
            //        },
            //    });

            /* var script1 = DebugPatchScriptGenarated(Campaigns.GetInsertCampaignScript());
             var script2 = DebugPatchScriptGenarated(Campaigns.GetMixedScript());
             var script3 = DebugPatchScriptGenarated(Campaigns.UpdateScoreAndResponseSentiment());
             var script4 = DebugPatchScriptGenarated(CSMPulse.GetInsertScript());
             var script5 = DebugPatchScriptGenarated(CSMPulse.GetUpdateScript());
             var script6 = DebugPatchScriptGenarated(CSMPulse.GetUpdateScoreScript());
             var script7 = DebugPatchScriptGenarated(SupportTickets.InsertScript());
             var script8 = DebugPatchScriptGenarated(SupportTickets.DeleteScript());*/
            /* var script9 = DebugPatchScriptGenarated(SupportTickets.UpdateScript());

             var script11 = DebugPatchScriptGenarated(Account.InsertInvoice());
             var script12 = DebugPatchScriptGenarated(Account.InsertInvoiceSummary());
             var script13 = DebugPatchScriptGenarated(Account.InsertSubscription());
             var script14 = DebugPatchScriptGenarated(Account.UpdateInvoice());
             var script15 = DebugPatchScriptGenarated(Account.UpdateSubscriptions());
             var script16 = DebugPatchScriptGenarated(Account.DeleteInvoice());
             var script17 = DebugPatchScriptGenarated(Account.DeleteInvoiceSummary());*/
            /*var script18 = DebugPatchScriptGenarated(Account.DeleteSubscription());*/
            var contactScript = DebugPatchScriptGenarated(Account.DeleteContactsCustomField());
            /*var script19 = DebugPatchScriptGenarated(Contacts.UpdateShield());

            var script20 = DebugPatchScriptGenarated(CustomFields.AddTextCustomField());
            var script21 = DebugPatchScriptGenarated(CustomFields.UpdateTextCustomField());
            var script22 = DebugPatchScriptGenarated(CustomFields.RemoveTextCustomField());

            var script23 = DebugPatchScriptGenarated(CustomFields.AddNumberCustomField());
            var script24 = DebugPatchScriptGenarated(CustomFields.UpdateNumberCustomField());
            var script25 = DebugPatchScriptGenarated(CustomFields.RemoveNumberCustomField());

            var script26 = DebugPatchScriptGenarated(Campaigns.GetDeleteCampaignsScript());
            var script27 = DebugPatchScriptGenarated(Account.AddSubscriptionCustomField());*/

            /*var script =
                script1 + "\n\n\n" +
                script2 + "\n\n\n" +
                script3 + "\n\n\n" +
                script4 + "\n\n\n" +
                script5 + "\n\n\n" +
                script6 + "\n\n\n" +
                script7 + "\n\n\n" +
                script8 + "\n\n\n" +
                script9 + "\n\n\n" +
                script11 + "\n\n\n" +
                script12 + "\n\n\n" +
                script13 + "\n\n\n" +
                script14 + "\n\n\n" +
                script15 + "\n\n\n" +
                script16 + "\n\n\n" +
                script17 + "\n\n\n" +
                script18 + "\n\n\n" +
                script19 + "\n\n\n" +
                script20 + "\n\n\n" +
                script21 + "\n\n\n" +
                script22 + "\n\n\n" +
                script23 + "\n\n\n" +
                script24 + "\n\n\n" +
                script25 + "\n\n\n" +
                script26 + "\n\n\n" +
                script27 + "\n\n\n";*/


            var campaignsInstanceId = GetPropertyName<CustomerSearch>(
                customer => customer.Contacts,
                GetPropertyName<CustomerContact>(
                    contact => contact.Campaigns,
                    GetPropertyName<CustomerContactCampaign>(
                        campaign => campaign.InstanceId)));
        }

        //var campaignsInstanceId = GetPropertyName<CustomerSearch>(
        //        customer => customer.Contacts,
        //        GetPropertyName<CustomerContact>(
        //            contact => contact.Campaigns,
        //            GetPropertyName<CustomerContactCampaign>(
        //                campaign => campaign.InstanceId)));

        //Output = contacts.campaigns.instanceId

        public static string GetPropertyName<T>(
            Expression<Func<T, object>> property,
            string childPath = null)
        {
            List<string> stack = new List<string>();
            LambdaExpression lambda = (LambdaExpression)property;
            MemberExpression memberExpression;

            if (lambda.Body is UnaryExpression)
            {
                UnaryExpression unaryExpression = (UnaryExpression)(lambda.Body);
                memberExpression = (MemberExpression)(unaryExpression.Operand);
            }
            else
            {
                memberExpression = (MemberExpression)(lambda.Body);
            }

            var current = memberExpression;

            while (current != null)
            {
                var name = JsonNamingPolicy.CamelCase.ConvertName(((PropertyInfo)current.Member).Name);
                stack.Add(name);

                if (current.Expression is MemberExpression)
                {
                    current = (MemberExpression)current.Expression;
                }
                else
                {
                    current = null;
                }
            }
            stack.Reverse();
            var result = string.Join('.', stack);

            if (string.IsNullOrEmpty(childPath))
            {
                return result;
            }

            return result + "." + childPath;
        }

        private static string DebugPatchScriptGenarated(PatchScriptMetadata patchScriptMetadata)
        {
            var scriptJson = Newtonsoft.Json.JsonConvert.SerializeObject(patchScriptMetadata.Script);
            var paramsJson = Newtonsoft.Json.JsonConvert.SerializeObject(patchScriptMetadata.Params);

            //return (updateScriptPainlessScriptJson + updateScriptPainlessParamsJson);
            return scriptJson;
        }

        private class SampleObject
        {
            public string Name { get; set; }

            public int Id { get; set; }

            public SampleObject()
            {
                this.Name = "test";
                this.Id = 1;
            }
        }

        private class CSMPulse
        {
            public static PatchScriptMetadata GetInsertScript()
            {
                return PatchScriptMetadata.GetPatchScriptMetadata(
                    new List<FieldPatchDescriptor>
                    {
                        new FieldPatchDescriptor{
                            FieldPath = "sentiment.csmPulse.latest",
                            FieldPatchType = FieldPatchTypes.ReplaceExistingValues,
                            PatchValue = new SampleObject(),
                        }
                    });
            }

            public static PatchScriptMetadata GetUpdateScoreScript()
            {
                return PatchScriptMetadata.GetPatchScriptMetadata(
                    new List<FieldPatchDescriptor>
                    {
                        new FieldPatchDescriptor{
                            FieldPath = "sentiment.csmPulse.latest.score",
                            FieldPatchType = FieldPatchTypes.ReplaceExistingValues,
                            PatchValue = 200,
                        },
                    });
            }

            public static PatchScriptMetadata GetUpdateScript()
            {
                return PatchScriptMetadata.GetPatchScriptMetadata(
                    new List<FieldPatchDescriptor>
                    {
                        new FieldPatchDescriptor{
                            FieldPath = "sentiment.csmPulse.latest",
                            FieldPatchType = FieldPatchTypes.ReplaceExistingValues,
                            PatchValue = new SampleObject(),
                        },
                    });
            }
        }

        private class SupportTickets
        {
            public static PatchScriptMetadata InsertScript()
            {
                return PatchScriptMetadata.GetPatchScriptMetadata(
                    new List<FieldPatchDescriptor>
                    {
                        new FieldPatchDescriptor
                        {
                            FieldPath = "support.tickets",
                            FieldPatchType = FieldPatchTypes.Append,
                            PatchValue = new SampleObject()
                        },
                    });
            }

            public static PatchScriptMetadata DeleteScript()
            {
                return PatchScriptMetadata.GetPatchScriptMetadata(
                    new List<FieldPatchDescriptor>
                    {
                        new FieldPatchDescriptor
                        {
                            FieldPath = "support.tickets",
                            FieldPatchType = FieldPatchTypes.Remove,
                            PatchValue = "1kjanf",
                            FieldSelector = new FieldSelector()
                            {
                                FieldPath = "support.tickets.id",
                                Selector = 1,
                            }
                        },
                    });
            }

            public static PatchScriptMetadata UpdateScript()
            {
                return PatchScriptMetadata.GetPatchScriptMetadata(new List<FieldPatchDescriptor>
                {
                    new FieldPatchDescriptor
                       {
                           FieldPath = "support.tickets",
                           FieldSelector = new FieldSelector
                           {
                               FieldPath = "support.tickets.id",
                               Selector = 1,
                           },
                           FieldPatchType = FieldPatchTypes.ReplaceExistingValues,
                           PatchValue = new SampleObject()
                       },
                });
            }
        }

        private class Campaigns
        {
            public static PatchScriptMetadata UpdateScoreAndResponseSentiment()
            {
                var fieldSelector = new FieldSelector
                {
                    FieldPath = "contacts.id",
                    Selector = 1000,
                    Child = new FieldSelector
                    {
                        FieldPath = "campaigns.id",
                        Selector = 2
                    }
                };

                return PatchScriptMetadata.GetPatchScriptMetadata(new List<FieldPatchDescriptor>
                {
                    new FieldPatchDescriptor
                    {
                        FieldPath = "contacts.campaigns.score",
                        FieldPatchType = FieldPatchTypes.ReplaceExistingValues,
                        PatchValue = 5,
                        FieldSelector = fieldSelector,
                    },
                new FieldPatchDescriptor
                    {
                        FieldPath = "contacts.campaigns.responderSentiment",
                        FieldPatchType = FieldPatchTypes.ReplaceExistingValues,
                        PatchValue = "passives",
                        FieldSelector = fieldSelector,
                    },
                });

            }

            public static PatchScriptMetadata GetInsertCampaignScript()
            {
                return PatchScriptMetadata.GetPatchScriptMetadata(new List<FieldPatchDescriptor>
                {
                    new FieldPatchDescriptor
                    {
                        FieldPath = "contacts.campaigns",
                        FieldPatchType = FieldPatchTypes.Remove,
                        PatchValue = new List<object> {  new SampleObject() },
                        FieldSelector = new FieldSelector
                        {
                            FieldPath = "contacts.id",
                            Selector = 413,
                        },
                    },
                });
            }

            public static PatchScriptMetadata GetDeleteCampaignsScript()
            {
                return PatchScriptMetadata.GetPatchScriptMetadata(new List<FieldPatchDescriptor>
                {
                    new FieldPatchDescriptor
                    {
                        FieldPath = "contacts.campaigns",
                        FieldPatchType = FieldPatchTypes.ReplaceExistingValues,
                        PatchValue = new List<object> { }
                    },
                });
            }

            public static PatchScriptMetadata GetMixedScript()
            {
                var fieldSelector = new FieldSelector
                {
                    FieldPath = "externalId",
                    Selector = "1",
                };

                return PatchScriptMetadata.GetPatchScriptMetadata(new List<FieldPatchDescriptor>
                {
                    new FieldPatchDescriptor{
                        FieldPatchType = FieldPatchTypes.ReplaceExistingValues,
                        FieldPath = "contacts",
                        PatchValue = new List<SampleObject> { new SampleObject() }
                    },
                    //new FieldPatchDescriptor("name", "Name", FieldPatchTypes.ReplaceExistingValues, fieldSelector),
                    //new FieldPatchDescriptor("countryId", "1", FieldPatchTypes.ReplaceExistingValues, fieldSelector),
                    //new FieldPatchDescriptor(
                    //    "contacts.campaigns",
                    //    new CustomerContactCampaign
                    //    {
                    //        Id = 5,
                    //        FromEmail = "yest@test.com",
                    //        InstanceId = "EG100266248655717409678733330",
                    //        RespondedOn = new DateTime(),
                    //        TriggeredAt = new DateTime()
                    //    },
                    //    FieldPatchTypes.Append,
                    //    new FieldSelector
                    //    {
                    //        FieldPath = "contacts.id",
                    //        Selector = 1
                    //    }),
                    //new FieldPatchDescriptor
                    //    {
                    //        FieldPath = "contacts.campaigns.score",
                    //        FieldPatchType = FieldPatchTypes.ReplaceExistingValues,
                    //        PatchValue = new SampleObject(),
                    //        FieldSelector = new FieldSelector
                    //        {
                    //            FieldPath = "contacts.id",
                    //            Selector = 1,
                    //            Child = new FieldSelector
                    //                        {
                    //                            FieldPath = "contacts.campaigns.instanceId",
                    //                            Selector = 2,
                    //                        },
                    //        },
                    //    },
                    //new FieldPatchDescriptor
                    //    {
                    //        FieldPath = "contacts.campaigns.response.followupAnswer",
                    //        FieldPatchType = FieldPatchTypes.ReplaceExistingValues,
                    //        PatchValue = new SampleObject(),
                    //        FieldSelector = new FieldSelector
                    //        {
                    //            FieldPath = "contacts.id",
                    //            Selector = 1,
                    //            Child = new FieldSelector
                    //                        {
                    //                            FieldPath = "contacts.campaigns.instanceId",
                    //                            Selector = 2,
                    //                        },
                    //        },
                    //    }
                    });
            }

        }

        private class Account
        {
            public static PatchScriptMetadata InsertSubscription()
            {
                return PatchScriptMetadata.GetPatchScriptMetadata(
                        new List<FieldPatchDescriptor> {
                            new FieldPatchDescriptor{
                                FieldPath = "accounts.subscriptions",
                                PatchValue = new SampleObject(),
                                FieldPatchType = FieldPatchTypes.Append,
                            },
                            new FieldPatchDescriptor{
                                FieldPath = "accounts.subscriptions",
                                PatchValue = new List<object>{ new SampleObject() },
                                FieldPatchType = FieldPatchTypes.Append,
                            }
                        }
                    );
            }

            public static PatchScriptMetadata UpdateSubscriptions()
            {
                return PatchScriptMetadata.GetPatchScriptMetadata(
                            new List<FieldPatchDescriptor> {
                            new FieldPatchDescriptor{
                                FieldPath = "accounts.subscriptions.count",
                                PatchValue = 100,
                                FieldPatchType = FieldPatchTypes.ReplaceExistingValues,
                                FieldSelector = new FieldSelector{
                                    FieldPath ="accounts.subscriptions.id",
                                    Selector = 1,
                                }
                            },
                            new FieldPatchDescriptor{
                                FieldPath = "accounts.subscriptions.summary",
                                PatchValue = new SampleObject(),
                                FieldPatchType = FieldPatchTypes.ReplaceExistingValues,
                                FieldSelector = new FieldSelector{
                                    FieldPath ="accounts.subscriptions.id",
                                    Selector = 1,
                                }
                            }
                            }
                        );
            }

            public static PatchScriptMetadata DeleteSubscription()
            {
                return PatchScriptMetadata.GetPatchScriptMetadata(
                        new List<FieldPatchDescriptor> {
                            new FieldPatchDescriptor{
                                FieldPath = "accounts.subscriptions",
                                FieldPatchType = FieldPatchTypes.Remove,
                                FieldSelector = new FieldSelector{
                                    FieldPath ="accounts.subscriptions.id",
                                    Selector = 1,
                                }
                            }
                        }
                      );
            }

            public static PatchScriptMetadata InsertInvoice()
            {
                return PatchScriptMetadata.GetPatchScriptMetadata(
                        new List<FieldPatchDescriptor> {
                            new FieldPatchDescriptor{
                                FieldPath = "accounts.subscriptions.invoices",
                                PatchValue = new SampleObject(),
                                FieldPatchType = FieldPatchTypes.Append,
                                FieldSelector  = new FieldSelector {
                                    FieldPath = "accounts.subscriptions.id",
                                    Selector = 10,
                                }
                            }
                        }
                        );
            }

            public static PatchScriptMetadata UpdateInvoice()
            {
                return PatchScriptMetadata.GetPatchScriptMetadata(
                        new List<FieldPatchDescriptor> {
                            new FieldPatchDescriptor{
                                FieldPath = "accounts.subscriptions.invoices",
                                PatchValue = new SampleObject(),
                                FieldPatchType = FieldPatchTypes.ReplaceExistingValues,
                                FieldSelector  = new FieldSelector{
                                    FieldPath = "accounts.subscriptions.id",
                                    Selector = 10,
                                    Child = new FieldSelector{
                                        FieldPath = "accounts.subscriptions.invoices.id",
                                        Selector = 23,
                                    }
                                }
                            },
                            new FieldPatchDescriptor{
                                FieldPath = "accounts.subscriptions.invoices.createdDate",
                                PatchValue = new SampleObject(),
                                FieldPatchType = FieldPatchTypes.ReplaceExistingValues,
                                FieldSelector  = new FieldSelector{
                                    FieldPath = "accounts.subscriptions.id",
                                    Selector = 10,
                                    Child = new FieldSelector{
                                        FieldPath = "accounts.subscriptions.invoices.id",
                                        Selector = 23,
                                    }
                                }
                            }
}
);
            }

            public static PatchScriptMetadata DeleteInvoice()
            {
                return PatchScriptMetadata.GetPatchScriptMetadata(
                        new List<FieldPatchDescriptor> {
                            new FieldPatchDescriptor{
                                FieldPath = "accounts.subscriptions.invoices",
                                FieldPatchType = FieldPatchTypes.Remove,
                                FieldSelector  = new FieldSelector{
                                    FieldPath = "accounts.subscriptions.id",
                                    Selector = 10,
                                    Child = new FieldSelector{
                                        FieldPath = "accounts.subscriptions.invoices.id",
                                        Selector = 23,
                                }
                            }
} }
);
            }

            public static PatchScriptMetadata InsertInvoiceSummary()
            {
                return PatchScriptMetadata.GetPatchScriptMetadata(
                        new List<FieldPatchDescriptor> {
                            new FieldPatchDescriptor{
                                FieldPath = "accounts.subscriptions.invoices.summary",
                                PatchValue = new SampleObject(),
                                FieldPatchType = FieldPatchTypes.ReplaceExistingValues,
                                FieldSelector  = new FieldSelector{
                                    FieldPath = "accounts.subscriptions.id",
                                    Selector = 10,
                                    Child = new FieldSelector{
                                    FieldPath = "accounts.subscriptions.invoices.id",
                                    Selector = 23,
                                }
                            } } }
);
            }

            public static PatchScriptMetadata DeleteInvoiceSummary()
            {
                return PatchScriptMetadata.GetPatchScriptMetadata(
                        new List<FieldPatchDescriptor> {
                            new FieldPatchDescriptor{
                                FieldPath = "accounts.subscriptions.invoices.summary",
                                PatchValue = null,
                                FieldPatchType = FieldPatchTypes.ReplaceExistingValues,
                                FieldSelector  = new FieldSelector{
                                    FieldPath = "accounts.subscriptions.id",
                                    Selector = 10,
                                    Child = new FieldSelector{
                                        FieldPath = "accounts.subscriptions.invoices.id",
                                        Selector = 23,
                                }
                            } 
                        } 
                    }
                );
            }

            public static PatchScriptMetadata DeleteContactsCustomField()
            {
                FieldSelector childFieldSelector = new FieldSelector
                {
                    FieldPath = "contacts.customFields.texts",
                    Child = new FieldSelector
                    {
                        FieldPath = "contacts.customFields.texts.id",
                        Selector = "52b0fa4b-da4b-4c5c-947b-7a27ed39d873"
                    }
                };

                FieldSelector objectFieldSelector = new FieldSelector
                {
                    FieldPath = "contacts.customFields",
                    Child = childFieldSelector
                };

                FieldSelector nestedFieldSelector = new FieldSelector
                {
                    FieldPath = "contacts",
                    Child = objectFieldSelector
                };

                List<FieldPatchDescriptor> fieldPatchDescriptors = new List<FieldPatchDescriptor>
                                                                                {
                                                                                    new FieldPatchDescriptor
                                                                                    {
                                                                                        FieldPath = "contacts.customFields.texts",
                                                                                        FieldPatchType = FieldPatchTypes.Remove,
                                                                                        FieldSelector = nestedFieldSelector
                                                                                    }
                                                                                };

                /*return PatchScriptMetadata.GetPatchScriptMetadata(fieldPatchDescriptors);*/


                /* return PatchScriptMetadata.GetPatchScriptMetadata(
                         new List<FieldPatchDescriptor> {
                             new FieldPatchDescriptor{
                                 FieldPath = "contacts.customFields.texts",
                                 FieldPatchType = FieldPatchTypes.Remove,
                                 FieldSelector  = new FieldSelector{
                                     FieldPath = "contacts",
                                     Child = new FieldSelector{
                                         FieldPath = "contacts.customFields.texts.id",
                                         Selector = "52b0fa4b-da4b-4c5c-947b-7a27ed39d873",
                                         *//*Child = new FieldSelector
                                         {
                                             FieldPath = "contacts.customFields.texts.id",
                                             Selector = "52b0fa4b-da4b-4c5c-947b-7a27ed39d873",
                                         }*//*
                                 }
                             }
                         }
                     }
                 );*/

                return PatchScriptMetadata.GetPatchScriptMetadata(
                       new List<FieldPatchDescriptor> {
                            new FieldPatchDescriptor{
                                FieldPath = "contacts.customFields.texts",
                                FieldPatchType = FieldPatchTypes.Remove,
                                FieldSelector  = new FieldSelector
                                        {
                                           /* FieldPath = "contacts.customFields.texts",
                                            Child = new FieldSelector
                                            {*/
                                                FieldPath = "contacts.customFields.texts.id",
                                                Selector = "52b0fa4b-da4b-4c5c-947b-7a27ed39d873",
                                            /*}*/
                                        }
                        }
                   }
               );
            }

            public static PatchScriptMetadata AddSubscriptionCustomField()
            {
                return PatchScriptMetadata.GetPatchScriptMetadata(
                       new List<FieldPatchDescriptor> {
                            new FieldPatchDescriptor{
                                FieldPath = "accounts.subscriptions.customFields.texts",
                                PatchValue = new List<SampleObject> { new SampleObject() },
                                FieldPatchType = FieldPatchTypes.Append,
                                FieldSelector  = new FieldSelector {
                                    FieldPath = "accounts.subscriptions.id",
                                    Selector = 10,
                                }
                            }
                       }
                );
            }
        }

        private class Contacts
        {

            public static PatchScriptMetadata UpdateShield()
            {
                return PatchScriptMetadata.GetPatchScriptMetadata(
                            new List<FieldPatchDescriptor> {
                        new FieldPatchDescriptor
                         {
                            FieldPath = "contacts.shield",
                            FieldPatchType = FieldPatchTypes.ReplaceExistingValues,
                            PatchValue = "Advocate",
                            FieldSelector = new FieldSelector
                            {
                                FieldPath = "contacts.shield",
                                Selector = "Others",
                            },
                         }
                    }
                        );
            }

        }

        private class CustomFields
        {
            private static readonly string targetId = "textCustomField1";

            public static PatchScriptMetadata AddTextCustomField()
            {
                return AddCustomField("texts");
            }
            
            public static PatchScriptMetadata RemoveTextCustomField()
            {
                return RemoveCustomField("texts");
            }
            
            public static PatchScriptMetadata UpdateTextCustomField()
            {
                return UpdateCustomField("texts");
            }
            
            public static PatchScriptMetadata AddNumberCustomField()
            {
                return AddCustomField("numbers");
            }
            
            public static PatchScriptMetadata RemoveNumberCustomField()
            {
                return RemoveCustomField("numbers");
            }
            
            public static PatchScriptMetadata UpdateNumberCustomField()
            {
                return UpdateCustomField("numbers");
            }

            private static PatchScriptMetadata AddCustomField(string type)
            {
                return PatchScriptMetadata.GetPatchScriptMetadata(
                    new List<FieldPatchDescriptor> {
                        new FieldPatchDescriptor
                        {
                            FieldPath = $"customFields.{type}",
                            FieldPatchType = FieldPatchTypes.Append,
                            PatchValue = new List<SampleObject>{ new SampleObject() },
                        }
                    });
            }

            private static PatchScriptMetadata RemoveCustomField(string type)
            {
                return PatchScriptMetadata.GetPatchScriptMetadata(
                    new List<FieldPatchDescriptor> {
                        new FieldPatchDescriptor
                        {
                            FieldPath = $"customFields.{type}",
                            FieldPatchType = FieldPatchTypes.Remove,
                            PatchValue = targetId,
                        }
                    });
            }

            private static PatchScriptMetadata UpdateCustomField(string type)
            {
                return PatchScriptMetadata.GetPatchScriptMetadata(
                    new List<FieldPatchDescriptor> {
                        new FieldPatchDescriptor
                        {
                            FieldPath = $"customFields.{type}",
                            FieldPatchType = FieldPatchTypes.ReplaceExistingValues,
                            PatchValue = new SampleObject(),
                            FieldSelector = new FieldSelector
                            {
                                FieldPath = $"customFields.{type}.id",
                                Selector = targetId,
                            }
                        }
                    });
            }
        }
    }
}
