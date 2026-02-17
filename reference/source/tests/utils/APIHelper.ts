import axios from 'axios';
import { getEnvURL } from '../utils/Login'
import qs from 'qs';
import { URLS } from '../constants/TestUsers.json';
import { expect } from '@playwright/test'

export async function deleteRecord(entity: string, guid: string): Promise<void> {

    console.log(`Deleting record '${guid}' of '${entity}' entity`)
    const appURL = await getEnvURL();
    const token = await generateToken();
    var getAPIHeaders = await getHeadersForRecordReading(token);
    var url = `${appURL}/api/data/v9.2/${entity}(${guid})`;

    try {
        const getAPIResponse = await axios.get(url, {
            headers: getAPIHeaders
        });
        
        expect(getAPIResponse.status).toBe(200);
        expect(getAPIResponse.statusText).toBe('OK');
    } catch (error: any) {
        console.error('❌ Record not found :', error.response?.status);
        console.error('❌ Request failed with status:', error.response?.status);
        console.error('Details:', error.response?.data?.error || error.response?.data);
        throw error;
    }

    var postAPIHeaders = await getHeadersForRecordDeleting(token);
    try {
        const deleteAPIResponse = await axios.delete(url, {
            headers: postAPIHeaders
        });

        expect(deleteAPIResponse.status).toBe(204);
        expect(deleteAPIResponse.statusText).toBe('No Content');
        console.log(`✅ Record ${guid} deleted successfully.`);

    } catch (error: any) {
        if (axios.isAxiosError(error)) {
            console.error(`❌ Failed to delete record: ${error.response?.status} - ${error.response?.data?.error?.message || error.message}`);
        } else {
            console.error(`❌ Unexpected error: ${error}`);
        }
    }
}

export async function generateToken(): Promise<string> {
    let token;
    try {
        const tenantId = `${process.env.AZURE_TENANT_ID}`;
        const clientId = `${process.env.AZURE_CLIENT_ID}`;
        const clientSecret = `${process.env.AZURE_CLIENT_SECRET}`;
        const appURL = await getEnvURL();
        const tokenUrl = `${URLS.TOKEN_URL}` + `${tenantId}` + '/oauth2/v2.0/token';

        const data = {
            grant_type: 'client_credentials',
            client_id: clientId,
            client_secret: clientSecret,
            scope: `${appURL}/.default`,
        };

        const response = await axios.post(tokenUrl, qs.stringify(data), {
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        });
        token = response.data.access_token
    }
    catch (error) {
        console.log(`Error while generating token : ${(error as Error).message}`);
        throw error;
    }
    return token;
}

export async function getHeadersForRecordReading(token: string): Promise<Record<string, string>> {
    return {
        Authorization: `Bearer ${token}`,
        Accept: 'application/json',
        'OData-MaxVersion': '4.0',
        'OData-Version': '4.0',
        Prefer: 'odata.include-annotations="*"',
    };
}

export async function getHeadersForRecordDeleting(token: string): Promise<Record<string, string>> {
    return {
        Authorization: `Bearer ${token}`,
        Accept: 'application/json',
        'OData-MaxVersion': '4.0',
        'OData-Version': '4.0'
    };
}