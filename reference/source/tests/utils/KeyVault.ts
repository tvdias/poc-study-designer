import { SecretClient } from '@azure/keyvault-secrets';
import { DefaultAzureCredential } from '@azure/identity';

export async function getSecretFromKeyVault(testUser: string): Promise<String> {
    let user, pass, testUserDetails, secretNames;
    try {
        const credential = new DefaultAzureCredential();
        const client = new SecretClient(`${process.env.KEY_VAULT_URL}`, credential)
        switch (testUser) {
            case "CSUser":
                secretNames = [`${process.env.CS_USERNAME_SECRET}`, `${process.env.CS_PASS_SECRET}`];
                break;

            case "LibrarianUser":
                secretNames = [`${process.env.LIBRARIAN_USERNAME_SECRET}`, `${process.env.LIBRARIAN_PASS_SECRET}`];
                break;
            case "ScripterUser":
                secretNames = [`${process.env.SCRIPTER_USERNAME_SECRET}`, `${process.env.SCRIPTER_PASS_SECRET}`];
                break;

            case "AdminUser":
                secretNames = ['', ''];
                break;

            default:
                console.log(`Given ${testUser} is not found!`);
                break;
        }

        user = await client.getSecret(secretNames[0]);
        pass = await client.getSecret(secretNames[1]);
        testUserDetails = user.value + "," + pass.value;
    } catch (error) {
        console.log(`Error while fetching key vault : ${(error as Error).message}`);
        throw error;
    }
    return testUserDetails;
}