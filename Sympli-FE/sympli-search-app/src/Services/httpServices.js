import axios from 'axios';

const BASE_URL = 'https://localhost:44359/api';

const httpService = {
  getSeoResult: async (keywords, url) => {
    try {
      const response = await axios.get(`${BASE_URL}/seo-result`, {
        params: { keywords, url },
      });
      return response.data;
    } catch (error) {
      throw new Error(
        error.response?.data?.message || 'Failed to fetch data from server'
      );
    }
  },
};

export default httpService