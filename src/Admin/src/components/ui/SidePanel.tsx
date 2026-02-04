import React, { useEffect } from 'react';
import './SidePanel.css';

interface SidePanelProps {
    isOpen: boolean;
    onClose: () => void;
    title: string;
    children: React.ReactNode;
    footer?: React.ReactNode;
    width?: string;
}

export function SidePanel({ isOpen, onClose, title, children, footer, width = '400px' }: SidePanelProps) {
    useEffect(() => {
        const handleEscape = (e: KeyboardEvent) => {
            if (e.key === 'Escape' && isOpen) onClose();
        };
        window.addEventListener('keydown', handleEscape);
        return () => window.removeEventListener('keydown', handleEscape);
    }, [isOpen, onClose]);

    if (!isOpen) return null;

    return (
        <div className="side-panel-overlay" onClick={onClose}>
            <div
                className="side-panel"
                style={{ width }}
                onClick={(e) => e.stopPropagation()} // Prevent closing when clicking inside
            >
                <div className="side-panel-header">
                    <h3>{title}</h3>
                    <button className="icon-btn" onClick={onClose} aria-label="Close">✕</button>
                </div>

                <div className="side-panel-content">
                    {children}
                </div>

                {footer && (
                    <div className="side-panel-footer">
                        {footer}
                    </div>
                )}
            </div>
        </div>
    );
}
