import * as React from 'react';

export interface InvoiceContactTeam {
  contactEmail: string;
  contactPhoneNumber: string;
}

interface InvoiceContactProps {
  team: InvoiceContactTeam;
  externalReferenceUrl: string | null;
  externalReferenceLabel: string | null;
}

const InvoiceContact = ({
  team,
  externalReferenceUrl,
  externalReferenceLabel
}: InvoiceContactProps) => {
  if (externalReferenceUrl) {
    return (
      <div className='pay-footer'>
        <span>
          If you have any questions, see{' '}
          <a
            href={externalReferenceUrl}
            target='_blank'
            rel='noopener noreferrer'
          >
            {externalReferenceLabel}
          </a>
          .
        </span>
      </div>
    );
  }

  return (
    <div className='pay-footer'>
      <span>
        If you have any questions, contact us
        {team.contactEmail && (
          <>
            {' at '}
            <a href={`mailto:${team.contactEmail}`}>{team.contactEmail}</a>
          </>
        )}
        {team.contactEmail && team.contactPhoneNumber && ' or'}
        {team.contactPhoneNumber && <> call at {team.contactPhoneNumber}</>}
        {!team.contactEmail && !team.contactPhoneNumber && '.'}
      </span>
    </div>
  );
};

export default InvoiceContact;
