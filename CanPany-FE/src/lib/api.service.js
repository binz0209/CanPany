import api from './api';

/**
 * API Service - Thống nhất cách gọi API và xử lý response
 * Backend trả về ApiResponse<T> với structure: { success, message, data, errorCode, ... }
 */

/**
 * Extract data from ApiResponse
 * @param {*} response - Axios response object
 * @returns {*} - Data từ response.data.data hoặc response.data
 */
export const extractData = (response) => {
  // Backend trả về ApiResponse<T> với data trong response.data.data
  if (response?.data?.data !== undefined) {
    return response.data.data;
  }
  // Fallback cho trường hợp không dùng ApiResponse
  return response?.data;
};

/**
 * Extract error message from error response
 * @param {*} error - Axios error object
 * @returns {string} - Error message
 */
export const extractErrorMessage = (error) => {
  if (error?.response?.data?.message) {
    return error.response.data.message;
  }
  if (error?.response?.data?.error) {
    return error.response.data.error;
  }
  if (error?.message) {
    return error.message;
  }
  return 'Đã xảy ra lỗi';
};

/**
 * API Helper Functions
 */
export const apiService = {
  /**
   * GET request
   */
  get: async (url, config = {}) => {
    try {
      // Nếu config có params, chuyển thành query string hoặc dùng axios config
      const response = await api.get(url, config);
      return extractData(response);
    } catch (error) {
      throw {
        ...error,
        message: extractErrorMessage(error),
      };
    }
  },

  /**
   * POST request
   */
  post: async (url, data = {}, config = {}) => {
    try {
      const response = await api.post(url, data, config);
      return extractData(response);
    } catch (error) {
      throw {
        ...error,
        message: extractErrorMessage(error),
      };
    }
  },

  /**
   * PUT request
   */
  put: async (url, data = {}, config = {}) => {
    try {
      const response = await api.put(url, data, config);
      return extractData(response);
    } catch (error) {
      throw {
        ...error,
        message: extractErrorMessage(error),
      };
    }
  },

  /**
   * PATCH request
   */
  patch: async (url, data = {}, config = {}) => {
    try {
      const response = await api.patch(url, data, config);
      return extractData(response);
    } catch (error) {
      throw {
        ...error,
        message: extractErrorMessage(error),
      };
    }
  },

  /**
   * DELETE request
   */
  delete: async (url, config = {}) => {
    try {
      const response = await api.delete(url, config);
      return extractData(response);
    } catch (error) {
      throw {
        ...error,
        message: extractErrorMessage(error),
      };
    }
  },

  /**
   * Upload file (multipart/form-data)
   */
  upload: async (url, formData, config = {}) => {
    try {
      const response = await api.post(url, formData, {
        ...config,
        headers: {
          'Content-Type': 'multipart/form-data',
          ...config.headers,
        },
      });
      return extractData(response);
    } catch (error) {
      throw {
        ...error,
        message: extractErrorMessage(error),
      };
    }
  },
};

export default apiService;

