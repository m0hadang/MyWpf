using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotification.DumyRefrence
{
    /*
     * MyResource는 이미지 파일만 존재하기에 MyNorification 빌드시 출력 디렉터리에 MyResource는 같이 복사되지 않음
     * 복사가 될려면 MyResource 안에 클래스 파일을 두어 참조하고 있어야함
     */
    class DumyClass
    {
        public DumyClass()
        {
            MyResource.Anchor anchor = new MyResource.Anchor();
        }
    }
}
