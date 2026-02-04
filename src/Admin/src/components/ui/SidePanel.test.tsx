import { render, screen, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import { SidePanel } from './SidePanel';
import '@testing-library/jest-dom';

describe('SidePanel', () => {
    it('does not render when not open', () => {
        render(
            <SidePanel isOpen={false} onClose={() => { }} title="Test Panel">
                <div>Content</div>
            </SidePanel>
        );
        const content = screen.queryByText('Test Panel');
        expect(content).not.toBeInTheDocument();
    });

    it('renders correct title and content when open', () => {
        render(
            <SidePanel isOpen={true} onClose={() => { }} title="Test Panel">
                <div>Panel Content</div>
            </SidePanel>
        );
        expect(screen.getByText('Test Panel')).toBeInTheDocument();
        expect(screen.getByText('Panel Content')).toBeInTheDocument();
    });

    it('calls onClose when close button is clicked', () => {
        const onClose = vi.fn();
        render(
            <SidePanel isOpen={true} onClose={onClose} title="Test Panel">
                <div>Content</div>
            </SidePanel>
        );

        const closeBtn = screen.getByRole('button', { name: /close/i });
        fireEvent.click(closeBtn);
        expect(onClose).toHaveBeenCalledTimes(1);
    });

    it('calls onClose when overlay is clicked', () => {
        const onClose = vi.fn();
        const { container } = render(
            <SidePanel isOpen={true} onClose={onClose} title="Test Panel">
                <div>Content</div>
            </SidePanel>
        );

        // The first div is the overlay
        // eslint-disable-next-line testing-library/no-container, testing-library/no-node-access
        const overlay = container.querySelector('.side-panel-overlay');
        if (overlay) fireEvent.click(overlay);
        expect(onClose).toHaveBeenCalledTimes(1);
    });

    it('does not call onClose when content is clicked', () => {
        const onClose = vi.fn();
        render(
            <SidePanel isOpen={true} onClose={onClose} title="Test Panel">
                <div>Content</div>
            </SidePanel>
        );

        const content = screen.getByText('Content');
        fireEvent.click(content);
        expect(onClose).not.toHaveBeenCalled();
    });

    it('calls onClose when Escape key is pressed', () => {
        const onClose = vi.fn();
        render(
            <SidePanel isOpen={true} onClose={onClose} title="Test Panel">
                <div>Content</div>
            </SidePanel>
        );

        fireEvent.keyDown(window, { key: 'Escape', code: 'Escape' });
        expect(onClose).toHaveBeenCalledTimes(1);
    });
});
