import { BrowserContext, Page } from "@playwright/test";
import variables from "../variables/variables.json";
import { getSecretFromKeyVault } from '../utils/KeyVault';
import { URLS, Environment } from '../constants/TestUsers.json';
import { Common } from '../selectors/CommonSelector.json';
import { WebHelper } from './../utils/WebHelper';



import { BasePage } from "./BasePage";
import {
  userName_TextBox,
  submit_Button,
  password_TextBox,
} from "../pageObjects/Login";

export class LoginPage extends BasePage {
  readonly page: Page;
  readonly context: BrowserContext;

  constructor(page: Page, context: BrowserContext) {
    super(page);
    this.context = context;
    this.page = page;
  }

  async navigateToURL(): Promise<void> {
    let url = "";
    if (Environment.Env.toUpperCase() == 'DEV') {
      url = URLS.UC1_Dev_Candidate_URL;
    } else if (Environment.Env.toUpperCase() == 'TEST') {
      url = URLS.UC1_Test_URL;
    }

    await super.open(url);
  }

  async loginToApplication(testUser: string): Promise<void> {
    const webHelper = new WebHelper(this.page);

    let userName, password, loginDetails, loginCredentials;
    loginDetails = await getSecretFromKeyVault(testUser);
    loginCredentials = loginDetails.split(",");
    userName = loginCredentials[0];
    password = loginCredentials[1];

    await this.page.waitForSelector(userName_TextBox, { state: "visible" });
    await this.fillTextBox(userName_TextBox, userName);

    await this.page.waitForSelector(submit_Button, { state: "visible" });
    await this.page.click(submit_Button);

    await this.page.waitForSelector(password_TextBox, { state: "visible" });
    await this.fillTextBox(password_TextBox, password);
    await this.page.waitForSelector(submit_Button, { state: "visible" });
    await this.page.click(submit_Button);
  }
}
