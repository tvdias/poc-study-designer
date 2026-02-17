## Power Apps - PCF Controls

PCF Controls are build with React.

### Installing PAC CLI
To publish a PCF component, you need to install the Power Platform CLI (PAC CLI):

- **Using .exe installer:** [Microsoft Docs](https://learn.microsoft.com/en-us/power-platform/developer/howto/install-cli-msi)
- or **Using VS Code Extension:** [Microsoft Docs](https://learn.microsoft.com/en-us/power-platform/developer/howto/install-vs-code-extension)

### Pre-requisites
1. **Install NVM for Windows** (Node Version Manager): [GitHub Repository](https://github.com/coreybutler/nvm-windows#installation--upgrades)
2. **Install Node.js** (Ensure compatibility: for e.g., PCF FlexibleOrderingGrid requires Node v16)
3. **Install PAC CLI** (Works only on Windows, not on Linux) - *npm install -g @microsoft/powerapps-cli*

### How to run a PCF component locally
1. Enter the folder of the PCF component you want to run. For e.g. 'FlexibleOrderingGrid'
2. Open file 'package-json'. This file contains a config json where every project configurations are setted.
3. Under json object "scripts", you'll see every command that is available for this project
4. Open PowerShell terminal under the same directory as that 'package.json' file
5. Run command to install dependencies:
```sh
npm install
```
6. Now you're ready to run the commands available in package.json:
```sh
npm run clean
npm run build
npm run start
```

### How to publish a PCF Component
```sh
npm install
npm run clean
npm run build
pac auth create --environment "ed2b63a6-8e22-edf1-8567-45353442985b"
pac pcf push --solution-unique-name "QuestionBank" --environment "ed2b63a6-8e22-edf1-8567-45353442985b"
```

### How to create a PCF Component

Run the following commands:

* There's 2 types of templates:
    * *dataset* -  For dataset-bound controls (bound to a grid or list of records);
    * *field* - For single field-bound controls (replaces a single field’s UI).
```sh
pac pcf init --namespace Kantar.Controls --name NameOfYourPCFComponent --template field/dataset
```
```sh
cd NameOfYourPCFComponent
npm install
npm run build
npm start watch
```

### Solution Versioning

```sh
pac solution version --buildversion [Buildnumber]
```

### Unit Testing

For Unit Tests we are using Jest library.

1 - First init config for Unit Tests:
```sh
npx ts-jest config:init
```
This creates a *jest.config.js*.
2 - Add a Test example under folder *__tests__*. E.g.: *__tests__/MyControl.test.tsx*
```typescript
import React from "react";
import { render, screen } from "@testing-library/react";
import MyControl from "../MyControl";

describe("MyControl", () => {
  it("renders with default text", () => {
    render(<MyControl />);
    expect(screen.getByText("Hello World")).toBeInTheDocument();
  });
});
```

3 - Run Tests
```sh
npm test
```

4 - Mocking the PCF Context

PCFs depend on the Power Apps control framework context (context.parameters, etc.).
Pass *mockContext* into your control’s methods (e.g., *init*, *updateView*).
You can unit test logic by mocking it:

```typescript
const mockContext: any = {
  parameters: {
    sampleInput: { raw: "test value" }
  },
  userSettings: { userId: "123" }
};
```