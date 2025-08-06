import * as React from 'react';

declare var $: JQueryStatic;

interface IProps {
  value: string;
  onChange: (value: string) => void;
  startDate?: Date;
  placeholder: string;
  required?: boolean;
}

// from @types/bootstrap-datepicker
// https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/types/bootstrap-datepicker/index.d.ts
interface DatepickerEventObject extends JQueryEventObject {
  date: Date;
  dates: Date[];
  format(ix?: number): string;
  format(format?: string): string;
  format(ix?: number, format?: string): string;
}

export default class DateControl extends React.Component<IProps, {}> {
  private _datePicker: HTMLElement;

  constructor(props) {
    super(props);

    this.state = {};
  }

  public componentDidMount() {
    $(this._datePicker).on('changeDate', this.handleDateChangeEvent);
  }

  public componentWillUnmount() {
    $(this._datePicker).off('changeDate', this.handleDateChangeEvent);
  }

  public render() {
    const { value, startDate, placeholder, required } = this.props;

    const datePickerOptions = {
      'data-date-container': 'body',
      'data-date-start-date': startDate
    };

    return (
      <div
        className='input-group date'
        data-provide='datepicker'
        {...datePickerOptions}
        ref={r => (this._datePicker = r)}
      >
        <input
          type='text'
          className='form-control'
          placeholder={placeholder}
          value={value}
          onChange={e => this.onChange(e.target.value)}
          required={required}
        />
        <div className='input-group-text'>
          <button className='btn' type='button'>
            <i className='far fa-fw fa-calendar' />
          </button>
        </div>
      </div>
    );
  }

  // triggered by date changes from the picker
  private handleDateChangeEvent = (e: DatepickerEventObject) => {
    this.onChange(e.format());
    return false;
  };

  private onChange = (value: string) => {
    const { onChange } = this.props;
    if (onChange) {
      onChange(value);
    }
  };
}
