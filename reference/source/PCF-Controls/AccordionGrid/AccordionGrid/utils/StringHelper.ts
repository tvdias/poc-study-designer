export function stripHtml(html: string): string {
    if (!html) return "";
    const doc = new DOMParser().parseFromString(html, "text/html");
    return doc.body.textContent || "";
}

export function sanitizeGuid(id?: string): string | undefined {
    return id ? id.replace(/[{}]/g, "").toLowerCase() : undefined;
}