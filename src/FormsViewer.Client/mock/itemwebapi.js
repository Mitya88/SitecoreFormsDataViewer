module.exports = {
  'POST /sitecore/api/ssc/formsviewerapi/statistics': function (req, res) {
    res.json({
      "FormId": "886a64f3-80c3-4083-bf3b-ae4765837ad4",
      "SuccessSubmits": 3,
      "SubmitsCount": 4,
      "Errors": 1,
      "Dropouts": 1,
      "Visits": 4,
      "DropoutRate": 25
  });
  },
  'POST /sitecore/api/ssc/formsviewerapi/detail': function (req, res) {
    res.json({
      "Headers": [
          "Created",
          "FirstName",
          "BirthDay",
          "Attachments",
          "Last Name",
          "Bio"
      ],
      "Entries": [
          [
              "5/20/2020 7:46:46 AM",
              "Mihály2",
              "2/5/2020 12:00:00 AM",
              "<a target=\"_blank\" href=\"https://sc93sc.dev.local/sitecore/api/ssc/forms/formdata/formdata/DownloadFile?fileId=34084179-b8cf-4501-a99d-89521c17e8da\">Download</a><br><a target=\"_blank\" href=\"https://sc93sc.dev.local/sitecore/api/ssc/forms/formdata/formdata/DownloadFile?fileId=dad79cc4-473a-405a-a7a5-b5770b0b8c47\">Download</a><br><a target=\"_blank\" href=\"https://sc93sc.dev.local/sitecore/api/ssc/forms/formdata/formdata/DownloadFile?fileId=6adc96fe-e21e-45a6-b939-91a9c2fe4ba8\">Download</a><br><a target=\"_blank\" href=\"https://sc93sc.dev.local/sitecore/api/ssc/forms/formdata/formdata/DownloadFile?fileId=ddfd292b-743f-4876-ad42-1a86c01ad6ff\">Download</a><br>",
              "Árvai2",
              "lorem ipsum dolor"
          ],
          [
              "5/20/2020 7:46:20 AM",
              "Mihály",
              "5/13/2020 12:00:00 AM",
              "<a target=\"_blank\" href=\"https://sc93sc.dev.local/sitecore/api/ssc/forms/formdata/formdata/DownloadFile?fileId=f4f9ffa3-69c0-4e38-9f4b-0b17ea01f7f6\">Download</a>",
              "Árvai",
              "bio"
          ],
          [
              "5/20/2020 7:53:24 AM",
              "asd",
              "1/10/2020 12:00:00 AM",
              "<a target=\"_blank\" href=\"https://sc93sc.dev.local/sitecore/api/ssc/forms/formdata/formdata/DownloadFile?fileId=aba2eb1a-64e2-49db-8d98-955f12b79403\">Download</a>",
              "dddd",
              "asd"
          ]
      ]
  });
  },
  'GET /sitecore/api/ssc/formsviewerapi/forms': function (req, res) {
    res.json([
      {
          "Name": "form3",
          "Id": "{5F1C35B1-FBAF-4C15-A33B-32AFACCFC386}"
      },
      {
          "Name": "Sample",
          "Id": "{886A64F3-80C3-4083-BF3B-AE4765837AD4}"
      },
      {
          "Name": "form2",
          "Id": "{99958CB0-7828-4B5E-9C52-5CB77587A66C}"
      }
  ]);
  }
};
