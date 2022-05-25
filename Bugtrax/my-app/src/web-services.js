import axios from 'axios';
// import moment from 'moment';


export function getAppURL() {
   // console.log('location: ' + window.location.href);
    if (window.location.href.toLowerCase().indexOf('localhost') > -1) {
        return '/api/WS/'//can do this with proxy set in package.json
    }
    else {
        return '../api/WS/';
    }
}

export async function getProjects() {
    const url = getAppURL() + 'GetProjects';
    const res = await axios.get(url);
    return res;
}