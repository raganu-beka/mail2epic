import {useState} from 'react';
import type {HistoryRow, RequiredAction} from '../types/models';

interface HistoryTableProps {
    rows: HistoryRow[];
    page: number;
    pageSize: number;
    onPrev: () => void;
    onNext: () => void;
}

function formatDate(value: string): string {
    const d = new Date(value);
    const dd = String(d.getDate()).padStart(2, '0');
    const mm = String(d.getMonth() + 1).padStart(2, '0');
    const yyyy = d.getFullYear();
    const HH = String(d.getHours()).padStart(2, '0');
    const MM = String(d.getMinutes()).padStart(2, '0');
    return `${dd}-${mm}-${yyyy} ${HH}:${MM}`;
}

function formatAction(action: RequiredAction | number): string {
    switch (action) {
        case 'AppendExistingIssue': case 0: return 'Appended';
        case 'CreateNewIssue': case 1: return 'New Issue';
        case 'NoAction': case 2: return 'No Action';
        default: return String(action);
    }
}

function actionBadgeClass(action: RequiredAction | number): string {
    switch (action) {
        case 'AppendExistingIssue': case 0:
            return 'bg-teal-100 text-teal-700';
        case 'CreateNewIssue': case 1:
            return 'bg-violet-100 text-violet-700';
        case 'NoAction': case 2:
            return 'bg-gray-100 text-gray-500';
        default:
            return 'bg-gray-100 text-gray-500';
    }
}

function ActionBadge({ action }: { action: RequiredAction | number }) {
    return (
        <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium whitespace-nowrap ${actionBadgeClass(action)}`}>
            {formatAction(action)}
        </span>
    );
}

function AiDetailsPanel({ row, onClose }: { row: HistoryRow; onClose: () => void }) {
    const { aiDecisionDetails } = row;
    return (
        <div className="border-t border-blue-100 bg-blue-50 px-4 py-4">
            <div className="flex items-start justify-between mb-2">
                <div>
                    <span className="text-xs font-semibold text-blue-700 uppercase tracking-wide">AI Decision Details</span>
                    <span className="ml-2 text-xs text-gray-500 truncate max-w-sm inline-block align-middle">{row.messageSubject}</span>
                </div>
                <button
                    onClick={onClose}
                    className="text-xs text-gray-400 hover:text-gray-600 transition-colors ml-4 shrink-0"
                >
                    ✕ Close
                </button>
            </div>
            <div className="space-y-1 text-sm text-gray-700">
                <div className="flex flex-wrap gap-x-8 gap-y-1">
                    <div><span className="font-medium text-gray-500">Action Taken:</span> {formatAction(row.actionTaken)}</div>
                    {aiDecisionDetails.suggestedEpicKey && (
                        <div><span className="font-medium text-gray-500">Suggested Epic:</span> {aiDecisionDetails.suggestedEpicKey}</div>
                    )}
                    {aiDecisionDetails.epicConfidence != null && (
                        <div><span className="font-medium text-gray-500">Epic Confidence:</span> {aiDecisionDetails.epicConfidence}%</div>
                    )}
                </div>
                {aiDecisionDetails.actionReasoning && (
                    <div><span className="font-medium text-gray-500">Action Reasoning:</span> {aiDecisionDetails.actionReasoning}</div>
                )}
            </div>
        </div>
    );
}

export function HistoryTable({ rows, page, pageSize, onPrev, onNext }: HistoryTableProps) {
    const [selectedIndex, setSelectedIndex] = useState<number | null>(null);

    const handleRowClick = (i: number) => {
        setSelectedIndex(prev => prev === i ? null : i);
    };

    return (
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
            <div className="overflow-x-auto">
                <table className="w-full text-sm text-left">
                    <thead className="bg-gray-50 border-b border-gray-200">
                        <tr>
                            {['Email', 'Sender', 'Subject', 'Action', 'Jira Issue', 'Epic', 'Processed At'].map(h => (
                                <th key={h} className="px-3 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide whitespace-nowrap">{h}</th>
                            ))}
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-100">
                        {rows.map((row, i) => (
                            <tr
                                key={i}
                                onClick={() => handleRowClick(i)}
                                className={`cursor-pointer transition-colors ${selectedIndex === i ? 'bg-blue-50' : 'hover:bg-gray-50'}`}
                            >
                                <td className="px-3 py-2.5 whitespace-nowrap">
                                    {row.gmailEmailLink
                                        ? <a href={row.gmailEmailLink} target="_blank" rel="noreferrer" onClick={e => e.stopPropagation()}>Open</a>
                                        : <span className="text-gray-400">—</span>}
                                </td>
                                <td className="px-3 py-2.5 text-gray-700 max-w-[10rem] truncate">{row.messageSender}</td>
                                <td className="px-3 py-2.5 text-gray-700 max-w-[16rem] truncate">{row.messageSubject}</td>
                                <td className="px-3 py-2.5 whitespace-nowrap">
                                    <ActionBadge action={row.actionTaken} />
                                </td>
                                <td className="px-3 py-2.5 whitespace-nowrap">
                                    {row.jiraIssueLink
                                        ? <a href={row.jiraIssueLink} target="_blank" rel="noreferrer" onClick={e => e.stopPropagation()}>{row.jiraIssueKey}</a>
                                        : <span className="text-gray-400">{row.jiraIssueKey ?? '—'}</span>}
                                </td>
                                <td className="px-3 py-2.5 max-w-[10rem] truncate">
                                    {row.issueEpic?.link
                                        ? <a href={row.issueEpic.link} target="_blank" rel="noreferrer" onClick={e => e.stopPropagation()}>{row.issueEpic.summary ?? row.issueEpic.key ?? '—'}</a>
                                        : <span className="text-gray-400">{row.issueEpic?.summary ?? row.issueEpic?.key ?? '—'}</span>}
                                </td>
                                <td className="px-3 py-2.5 whitespace-nowrap text-gray-500 text-xs">{formatDate(row.processedAt)}</td>
                            </tr>
                        ))}
                        {rows.length === 0 && (
                            <tr>
                                <td colSpan={7} className="px-4 py-10 text-center text-gray-400 text-sm">No records found.</td>
                            </tr>
                        )}
                    </tbody>
                </table>
            </div>

            {selectedIndex !== null && rows[selectedIndex] && (
                <AiDetailsPanel row={rows[selectedIndex]} onClose={() => setSelectedIndex(null)} />
            )}

            <div className="flex items-center justify-between px-4 py-3 border-t border-gray-200 bg-gray-50">
                <button
                    onClick={onPrev}
                    disabled={page <= 1}
                    className="px-3 py-1.5 text-sm rounded border border-gray-300 text-gray-700 hover:bg-white disabled:opacity-40 transition-colors"
                >
                    ← Previous
                </button>
                <span className="text-sm text-gray-600">Page {page}</span>
                <button
                    onClick={onNext}
                    disabled={rows.length < pageSize}
                    className="px-3 py-1.5 text-sm rounded border border-gray-300 text-gray-700 hover:bg-white disabled:opacity-40 transition-colors"
                >
                    Next →
                </button>
            </div>
        </div>
    );
}
