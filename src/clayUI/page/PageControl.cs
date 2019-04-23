using foundation;
using gameSDK;
using UnityEngine.UI;

namespace clayui
{
    public class PageControl
    {
        private ClayButton prevBtn;
        private ClayButton nextBtn;

        private ClayButton firstBtn;
        private ClayButton lastBtn;

        private Text pageNumberTF;

        protected AbstractPage page;
        private PageList pageList;

        public void bindPage(AbstractPage page)
        {
            this.page = page;
            this.page.addEventListener(EventX.CHANGE, pageChangeHandler);

            if (pageList != null)
            {
                pageList.dataProvider=page.getCurrentPageData();
            }
            if (pageNumberTF != null)
            {
                this.pageNumberTF.text = (page.currentPage + 1.0f) + "/" + page.totalPage;
            }
        }

        public void bindButton(ClayButton prev, ClayButton next, ClayButton first= null, ClayButton last= null){
			if(prev!=null){
				prevBtn=prev;
				prevBtn.addEventListener(MouseEventX.CLICK,clickHandler);
			}
			
			if(next!=null){
				nextBtn=next;
				nextBtn.addEventListener(MouseEventX.CLICK, clickHandler);
            }
			
			if(first!=null){
				firstBtn=first;
				firstBtn.addEventListener(MouseEventX.CLICK, clickHandler);
            }
			
			if(last!=null){
				lastBtn=last;
				lastBtn.addEventListener(MouseEventX.CLICK, clickHandler);
            }
		}
        public void bindButton(Button prev, Button next, Button first = null, Button last = null)
        {
            if (prev != null)
            {
                prevBtn = new ClayButton(prev.gameObject);
            }

            if (next != null)
            {
                nextBtn = new ClayButton(next.gameObject); ;
            }

            if (first != null)
            {
                firstBtn = new ClayButton(first.gameObject); 
            }

            if (last != null)
            {
                lastBtn = new ClayButton(last.gameObject); ; ;
            }
            bindButton(prevBtn, nextBtn, firstBtn, lastBtn);
        }



        public void bindPageList(PageList pageList){
			this.pageList=pageList;

            if (page != null)
            {
                this.pageList.dataProvider=page.getCurrentPageData();
            }
		}

        public void bindPageNumber(Text pageNumberTF)
        {
            this.pageNumberTF = pageNumberTF;

            if (page != null)
            {
                this.pageNumberTF.text = (page.currentPage + 1.0f) + "/" + page.totalPage;
            }
        }


        public void pageTo(int index){
			if(page !=null){
				page.currentPage=index;
			}
		}

        private void clickHandler(EventX e)
        {
            if (page == null)
            {
                return;
            }

            ClayButton dis = e.target as ClayButton;
            if (dis.enabled == false)
            {
                return;
            }

            if (dis == prevBtn)
            {
                page.previousPage();
            }
            else if (dis == nextBtn)
            {
                page.nextPage();
            }
            else if (dis == firstBtn)
            {
                page.currentPage = 0;
            }
            else if (dis == lastBtn)
            {

                page.currentPage = page.totalPage - 1;
            }
        }

        protected void pageChangeHandler(EventX e)
        {
            bool hasPrevious = page.hasPreviousPage;
            bool hasNext = page.hasNextPage;

            setInteractiveEnabled(prevBtn, hasPrevious);

            setInteractiveEnabled(nextBtn, hasNext);

            setInteractiveEnabled(firstBtn, hasPrevious);

            setInteractiveEnabled(lastBtn, hasNext);

            if (this.pageList != null)
            {
                this.pageList.dataProvider=this.page.getCurrentPageData();
            }
            if (pageNumberTF != null)
            {
                int total = page.totalPage;
                int current;
                if (total == 0)
                {
                    current = 1;
                    total = 1;
                }
                else
                {
                    current = page.currentPage + 1;
                }
                pageNumberTF.text = current + "/" + total;
            }
        }

        protected void setInteractiveEnabled(ClayButton inter, bool enabled)
        {
            if (inter == null)
            {
                return;
            }
            inter.SetActive(enabled);
        }
    }
}