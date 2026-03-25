export type RequiredAction = 'AppendExistingIssue' | 'CreateNewIssue' | 'NoAction' | 0 | 1 | 2;

export interface AiDecisionDetails {
    suggestedEpicKey: string | null;
    epicConfidence: number | null;
    actionReasoning: string | null;
}

export interface IssueEpic {
    key: string | null;
    summary: string | null;
    link: string | null;
}

export interface HistoryRow {
    messageSender: string;
    messageSubject: string;
    jiraIssueKey: string | null;
    actionTaken: RequiredAction;
    processedAt: string;
    jiraIssueLink: string | null;
    gmailEmailLink: string;
    aiDecisionDetails: AiDecisionDetails;
    issueEpic: IssueEpic | null;
}

