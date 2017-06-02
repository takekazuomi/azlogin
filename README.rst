=========
 azlogin
=========

Azure Login OAuth 2.0 Device Flow Console Program.


Usage
=====

Run and get bearer token for each subscription

.. code:: posh

   $ .\azlogin
   To sign in, use a web browser to open the page https://aka.ms/devicelogin and enter the code G9HFZ4NCY to authenticate.
   ....
   [
      {
        "tenantId": "xxxxxxxx-xxxx-xxxxxxxxx-xxxxxxxxxxxx",
        "subscriptionId": "xxxxxxxx-xxxx-xxxxxxxxx-xxxxxxxxxxxx",
        "subscriptionName": "Developer Program Benefit",
        "bearer": "********************************************a"
      },
      {
        "tenantId": "xxxxxxxx-xxxx-xxxxxxxxx-xxxxxxxxxxxx",
        "subscriptionId": "xxxxxxxx-xxxx-xxxxxxxxx-xxxxxxxxxxxx",
        "subscriptionName": "foo",
        "bearer": "********************************************a"
      },
      {
        "tenantId": "xxxxxxxx-xxxx-xxxxxxxxx-xxxxxxxxxxxx",
        "subscriptionId": "xxxxxxxx-xxxx-xxxxxxxxx-xxxxxxxxxxxx",
        "subscriptionName": "kinmugi",
        "bearer": "********************************************a"
      }
   ]


Run and get bearer token for each subscription. Save the bearer token after logging in the variable and execute the API. For cutting bearer token, use jp (JMESPath).

.. code:: posh

   $ cat filter.jp
   [?subscriptionName == 'kinmugi'].bearer|[0]

   $ $bearer = (.\azlogin | jp -u -e filter.jp)

   $ curl "https://management.azure.com/subscriptions/xxxxxxxx-.../resourcegroups?api-version=2017-05-10" -H  "Authorization: Bearer $bearer"


See
===
* `Login UI が出せない Client の OAuth フロー (Azure AD) <https://blogs.msdn.microsoft.com/tsmatsuz/2016/03/12/azure-ad-device-profile-oauth-flow/>`_
* `Invoking an API protected by Azure AD from a text-only device <https://azure.microsoft.com/en-us/resources/samples/active-directory-dotnet-deviceprofile/>`_
