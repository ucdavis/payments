import * as React from 'react';

import { addDays } from 'date-fns';

import DateControl from './dateControl';

interface IProps {
    value?: string;
    onChange: (value: string) => void;
}

export default class DueDateControl extends React.Component<IProps, {}> {

    constructor(props) {
        super(props);

        this.state = {
        };
    }

    public render() {
        const { value, onChange } = this.props;
        const startDate = addDays(new Date(), 1);

        return (
            <DateControl
                placeholder="optional"
                value={value}
                onChange={onChange}
                startDate={startDate}
            />
        );
    }
}