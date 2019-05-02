import axios from 'axios'
import {removeTokens, setTokens, parseJwt} from 'services/tokens';
import Lock from 'js-lock';
import {store} from 'store';

import Vue from 'vue';
import { consoleTokens, consoleUserLogout, consoleRequestStart, consoleRequestUrl } from "../defination";

const lock = new Lock("request-lock");


const apiAxios = axios.create({baseURL: config.API, withCredentials: process.env.DEV});


apiAxios.interceptors.response.use(async rez => {
  await checkTokens(rez);
  return rez;
}, async rez => {
  await checkTokens(rez.response);
  //Vue.prototype.$errorNotify(rez);
  throw rez;
});


export default async function request(context, data) {

  const url = data.url;

  console.log(`%cRequest%c${url}`, consoleRequestStart, consoleRequestUrl, data);

  const sendAsJson = data.sendAsJson ?? false;

  const headers = {};

  if (data.skipLock) {
    if (checkLocalTokensExpire()) {
      headers['LongToken1'] = tokens.longToken.token;
    }
    return makeRequest();
  }

  return lock.lock(() => {
      const tokens = store.state.auth.tokens;

      if (checkLocalTokensExpire()) {
        headers['LongToken1'] = tokens.longToken.token;
        return makeRequest();
      }
    }
  ).then(x => {
    if (x)
      return x;
    else
      return makeRequest();
  });

  function checkLocalTokensExpire() {
    const tokens = store.state.auth.tokens;
    const rez = tokens && tokens.shortTokenExpiration < new Date(new Date().toUTCString());
    if (rez)
      console.log("%cTokens expire", consoleTokens);

    return rez;
  }

  function makeRequest() {

    const tokens = store.state.auth.tokens;

    if (tokens)
      headers['Authorization'] = `Bearer ${tokens.shortToken}`;

    let body = data.data;

    if (body) {
      if ((typeof body === 'object')) {
        if (body instanceof FormData) {

        } else if (sendAsJson === false) {
          body = ConvertObjectToFormData(body);
        } else {
          headers['Content-Type'] = 'application/json';
          body = JSON.stringify(body);
        }
      } else {
        headers['Content-Type'] = 'application/x-www-form-urlencoded';
      }
    }

    return apiAxios.post(url, body,
      {
        headers: headers,
      });

  }
}

function ConvertObjectToFormData(obj) {
  const formData = new FormData();

  for (const key in obj) {
    if (obj.hasOwnProperty(key)) {
      formData.append(key, obj[key]);
    }
  }

  return formData;
}

async function checkTokens(rez) {
  const tokensHeader = rez.headers.tokens;
  if (tokensHeader) {
    if(tokensHeader === "expire") {
      store.state.auth.tokens = null;
      removeTokens();
      store.commit('clearAllUserRelatedData');
      await store.dispatch('getAllCategories', {skipLock: true});

      console.info("%cTokens logout", consoleUserLogout);
    }
    else {
      const tokens = JSON.parse(tokensHeader);
      const exps = parseJwt(tokens.shortToken);

      tokens.shortTokenExpiration = new Date(exps.exp * 1000);

      if (store.state.auth.isPermanentLogin)
        setTokens(tokens);

      store.state.auth.tokens = tokens;

      console.info("%cTokens refreshed", consoleTokens);
    }
  }

  return rez;
}
