import './css/site.scss';
import './polyfills/array.js';
import * as dateFns from 'date-fns';

interface ExtraWindow extends Window {
    dateFns: any;
}

const extraWindow = window as ExtraWindow;

extraWindow.dateFns = {
    format: dateFns.format,

    getMonth: dateFns.getMonth,
    setMonth: dateFns.setMonth,

    getYear: dateFns.getYear,
    setYear: dateFns.setYear,
}
